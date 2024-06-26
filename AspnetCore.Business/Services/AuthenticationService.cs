using AspnetCore.Business.Models.Authentication;
using AspnetCore.Business.SubServices;
using AspnetCore.Data;
using AspnetCore.Data.Entities;
using AspnetCore.Utilities.Exceptions;
using AspnetCore.Utilities.Models;
using AspnetCore.Utilities.Results;
using AspnetCore.Utilities.SubServices;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AspnetCore.Business.Services;

public interface IAuthenticationService
{
    Task<IActionResult> Register(RegisterModel model);
    Task<IActionResult> Login(LoginModel model);
    Task<IActionResult> RefreshToken(RefreshTokenModel model);
    Task<IActionResult> GetCurrentUserCredentialInfo();
}

public class AuthenticationService : IAuthenticationService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    private readonly UnitOfWork _unitOfWork;
    private readonly IBCryptSubService _bCryptSubService;
    private readonly IJwtSubService _jwtSubService;

    private readonly IUserCredentialSubService _userCredentialSubService;

    public AuthenticationService(IHttpContextAccessor httpContextAccessor,
        UnitOfWork mainUnitOfWork,
        IBCryptSubService bCryptSubService,
        IJwtSubService jwtSubService,
        IUserCredentialSubService userCredentialSubService)
    {
        _httpContextAccessor = httpContextAccessor;
        _unitOfWork = mainUnitOfWork;
        _bCryptSubService = bCryptSubService;
        _jwtSubService = jwtSubService;
        _userCredentialSubService = userCredentialSubService;
    }

    public async Task<IActionResult> Register(RegisterModel model)
    {
        var oldUserCredential = await _unitOfWork.UserCredentialRepository.Query
            .Where(x => x.Username.ToLower() == model.Username.ToLower())
            .FirstOrDefaultAsync();
        if (oldUserCredential != null)
            throw new CustomException("Tên đăng nhập đã tồn tại", 400);

        _unitOfWork.BeginTransaction();

        await _unitOfWork.UserCredentialRepository.InsertAsync(new UserCredential
        {
            Username = model.Username,
            Password = _bCryptSubService.HashPassword(model.Password),
        });
        if (!await _unitOfWork.SaveChangesAsync())
            throw new CustomException("Tạo user credential không thành công", 500);

        await _unitOfWork.EndTransactionAsync();

        return new CustomResult("Tạo user credential thành công", model);
    }

    public async Task<IActionResult> Login(LoginModel model)
    {
        var userCredential = await _unitOfWork.UserCredentialRepository.Query
            .Where(x => x.Username.ToLower() == model.Username.ToLower())
            .FirstOrDefaultAsync() ?? throw new CustomException("Tên đăng nhập hoặc mật khẩu không chính xác", 400);

        // Kiểm tra password
        var isMatch = _bCryptSubService.IsMatchPssword(userCredential.Password, model.Password);
        if (!isMatch)
            throw new CustomException("Tên đăng nhập hoặc mật khẩu không chính xác", 400);

        //
        if (userCredential.RefreshToken == null || _jwtSubService.ValidateRefreshToken(userCredential.RefreshToken) == null)
        {
            userCredential.RefreshToken = _jwtSubService.CreateRefreshToken(new TokenClaimModel { UserId = userCredential.Id });
            _unitOfWork.UserCredentialRepository.Update(userCredential);
            if (!await _unitOfWork.SaveChangesAsync())
                throw new CustomException("Cập nhật refresh token không thành công", 500);
        }

        return new CustomResult("Đăng nhập thành công", new
        {
            AccessToken = _jwtSubService.CreateAccessToken(new TokenClaimModel { UserId = userCredential.Id }),
            userCredential.RefreshToken,
            UserCredential = new
            {
                userCredential.Id,
                userCredential.Username,
            }
        });
    }

    public async Task<IActionResult> RefreshToken(RefreshTokenModel model)
    {
        var accessToken = (_httpContextAccessor.HttpContext.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last())
            ?? throw new CustomException("Thiếu access token", 401);

        var tokenClaim = _jwtSubService.DecodeAccessToken(accessToken)
            ?? throw new CustomException("Access token không hợp lệ", 401);

        var refreshTokenClaim = _jwtSubService.ValidateRefreshToken(model.RefreshToken)
            ?? throw new CustomException("Refresh token không hợp lệ hoặc đã hết hạn", 401);

        var userCredential = await _unitOfWork.UserCredentialRepository.Query
            .Where(x => x.Id == tokenClaim.UserId && !string.IsNullOrEmpty(x.RefreshToken) && x.RefreshToken == model.RefreshToken)
            .FirstOrDefaultAsync() ?? throw new CustomException("Người dùng hiện tại không hợp lệ", 401);

        return new CustomResult("Refresh token thành công", new
        {
            AccessToken = _jwtSubService.CreateAccessToken(new TokenClaimModel { UserId = userCredential.Id }),
            userCredential.RefreshToken,
            UserCredential = new
            {
                userCredential.Id,
                userCredential.Username,
            }
        });
    }

    public async Task<IActionResult> GetCurrentUserCredentialInfo()
    {
        var accessToken = (_httpContextAccessor.HttpContext.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last())
            ?? throw new CustomException("Thiếu access token", 401);

        var userCredential = await _userCredentialSubService.QueryCurrentUserCredential()
            .Select(x => new
            {
                x.Id,
                x.Username,
                x.RefreshToken
            })
            .FirstOrDefaultAsync() ?? throw new CustomException("Người dùng hiện tại không hợp lệ", 401);

        return new CustomResult("Thành công", new
        {
            userCredential.RefreshToken,
            AccessToken = accessToken,
            UserCredential = new
            {
                userCredential.Id,
                userCredential.Username,
            }
        });
    }
}