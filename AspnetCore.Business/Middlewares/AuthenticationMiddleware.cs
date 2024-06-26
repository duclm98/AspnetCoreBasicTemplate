using AspnetCore.Data;
using Microsoft.AspNetCore.Http;

namespace AspnetCore.Business.Middlewares;

public class AuthenticationMiddleware
{
    private readonly RequestDelegate _next;

    public AuthenticationMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task Invoke(HttpContext httpContext, UnitOfWork unitOfWork)
    {
        await HandleAuthenticationMiddleware.InvokeAuthenticationMiddleware(httpContext, unitOfWork);
        await _next(httpContext);
    }
}