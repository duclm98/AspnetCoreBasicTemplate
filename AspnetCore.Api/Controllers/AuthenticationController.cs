using AspnetCore.Business.Models.Authentication;
using AspnetCore.Business.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace AspnetCore.Api.Controllers;

[ApiController]
[ApiVersion("1")]
[Route("v{version:apiVersion}/authentication")]
public class AuthenticationController : ControllerBase
{
    private readonly IAuthenticationService _authenticationService;

    public AuthenticationController(IAuthenticationService authenticationService)
    {
        _authenticationService = authenticationService;
    }

    [SwaggerOperation(Summary = "Đăng ký")]
    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] RegisterModel model) =>
        await _authenticationService.Register(model);

    [SwaggerOperation(Summary = "Đăng nhập")]
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginModel model) =>
        await _authenticationService.Login(model);

    [SwaggerOperation(Summary = "Refresh token")]
    [HttpPost("refresh-token")]
    [AllowAnonymous]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenModel model) =>
        await _authenticationService.RefreshToken(model);

    [SwaggerOperation(Summary = "Lấy thông tin user đang đăng nhập")]
    [HttpGet("current-user-credential")]
    public async Task<IActionResult> GetCurrentUserCredentialInfo() =>
        await _authenticationService.GetCurrentUserCredentialInfo();
}