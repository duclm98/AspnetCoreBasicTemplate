using AspnetCore.Data;
using AspnetCore.Utilities.Constans;
using AspnetCore.Utilities.Exceptions;
using AspnetCore.Utilities.SubServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.EntityFrameworkCore;

namespace AspnetCore.Business.Middlewares;

public class JwtMiddleware
{
    private readonly RequestDelegate _next;

    public JwtMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task Invoke(HttpContext httpContext, IJwtSubService jwtSubService)
    {
        HandleAuthenticationMiddleware.InvokeJwtMiddleware(httpContext, jwtSubService);
        await _next(httpContext);
    }
}

public static class HandleAuthenticationMiddleware
{
    private static readonly string[] IgnorePaths = new[]
    {
        "/v1/path",
    };

    public static void InvokeJwtMiddleware(HttpContext httpContext, IJwtSubService jwtSubService)
    {
        if (!IgnorePaths.Contains(httpContext.Request.Path.Value) && !AllowAnonymous(httpContext))
        {
            var accessToken = (httpContext.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last())
                ?? throw new CustomException("Thiếu access token", 401);

            var tokenClaim = jwtSubService.ValidateAccessToken(accessToken)
                ?? throw new CustomException("Access token không hợp lệ hoặc đã hết hạn", 401);

            httpContext.Items[SystemConstant.ExecutorIdKey] = tokenClaim.UserId;
        }
    }

    public static async Task InvokeAuthenticationMiddleware(HttpContext httpContext, UnitOfWork unitOfWork)
    {
        if (!IgnorePaths.Contains(httpContext.Request.Path.Value) && !AllowAnonymous(httpContext))
        {
            try
            {
                var executorId = httpContext.Items[SystemConstant.ExecutorIdKey]
                    ?? throw new CustomException("Access token không hợp lệ hoặc đã hết hạn", 401);

                var userCredentialId = Convert.ToInt32(executorId);
                var user = await unitOfWork.UserCredentialRepository.Query
                    .Where(x => x.Id == userCredentialId)
                    .FirstOrDefaultAsync() ?? throw new CustomException("Access token không hợp lệ hoặc đã hết hạn", 401);
            }
            catch
            {
                throw new CustomException("Access token không hợp lệ hoặc đã hết hạn", 401);
            }
        }
    }

    private static bool AllowAnonymous(HttpContext httpContext)
    {
        var endpointFeature = httpContext.Features.Get<IEndpointFeature>();
        var endpoint = endpointFeature?.Endpoint;
        return endpoint?.Metadata?.GetMetadata<IAllowAnonymous>() is not null;
    }
}