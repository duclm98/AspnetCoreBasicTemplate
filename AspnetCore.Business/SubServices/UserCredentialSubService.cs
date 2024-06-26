using AspnetCore.Data;
using AspnetCore.Data.Entities;
using AspnetCore.Utilities.Constans;
using AspnetCore.Utilities.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace AspnetCore.Business.SubServices;

public interface IUserCredentialSubService
{
    Task<int> GetCurrentUserCredentialId();
    IQueryable<UserCredential> QueryCurrentUserCredential();
}

public class UserCredentialSubService : IUserCredentialSubService
{

    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly UnitOfWork _unitOfWork;

    public UserCredentialSubService(IHttpContextAccessor httpContextAccessor,
        UnitOfWork mainUnitOfWork)
    {
        _httpContextAccessor = httpContextAccessor;
        _unitOfWork = mainUnitOfWork;
    }

    public async Task<int> GetCurrentUserCredentialId()
    {
        var executorId = _httpContextAccessor.HttpContext?.Items[SystemConstant.ExecutorIdKey]
            ?? throw new CustomException("Người dùng hiện tại không hợp lệ", 401);

        var userCredentialId = Convert.ToInt32(executorId);
        var isExistedUser = await _unitOfWork.UserCredentialRepository.Query
            .AnyAsync(x => x.Id == userCredentialId);
        if (!isExistedUser)
            throw new CustomException("Người dùng hiện tại không hợp lệ", 401);

        return userCredentialId;
    }

    public IQueryable<UserCredential> QueryCurrentUserCredential()
    {
        var executorId = _httpContextAccessor.HttpContext?.Items[SystemConstant.ExecutorIdKey]
           ?? throw new CustomException("Người dùng hiện tại không hợp lệ", 401);

        var userCredentialId = Convert.ToInt32(executorId);
        return _unitOfWork.UserCredentialRepository.Query
            .Where(x => x.Id == userCredentialId);
    }
}