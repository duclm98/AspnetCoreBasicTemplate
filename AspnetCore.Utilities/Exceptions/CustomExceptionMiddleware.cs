using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Net;

namespace AspnetCore.Utilities.Exceptions;

public class CustomExceptionMiddleware
{
    private readonly ILogger<CustomExceptionMiddleware> logger;
    private readonly RequestDelegate next;

    public CustomExceptionMiddleware(ILogger<CustomExceptionMiddleware> logger, RequestDelegate next)
    {
        this.logger = logger;
        this.next = next;
    }

    public async Task Invoke(HttpContext context)
    {
        try
        {
            await next(context);
            await LogInformation(context);
        }
        catch (CustomException ex)
        {
            await HandleExceptionAsync(context, ex);
            await LogErrorCustomException(context, ex);
        }
        catch (Exception exceptionObj)
        {
            await HandleExceptionAsync(context, exceptionObj);
            await LogErrorException(context, exceptionObj);
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, CustomException exception)
    {
        string result;
        context.Response.ContentType = "application/json";
        if (exception is CustomException)
        {
            result = new ErrorDetail()
            {
                Message = exception.Message,
                StatusCode = exception.StatusCode,
                Data = exception.Data2
            }.ToString();
            context.Response.StatusCode = exception.StatusCode;
        }
        else
        {
            result = new ErrorDetail()
            {
                Message = exception.Message,
                StatusCode = (int)HttpStatusCode.BadRequest
            }.ToString();
            context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
        }

        return context.Response.WriteAsync(result);
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";
        string result = new ErrorDetail()
        {
            Message = exception.Message,
            StatusCode = (int)HttpStatusCode.InternalServerError
        }.ToString();
        context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

        return context.Response.WriteAsync(result);
    }

    private async Task LogInformation(HttpContext context)
    {
        context.Request.Body.Seek(0, SeekOrigin.Begin);
        string requestBody = await new StreamReader(context.Request.Body).ReadToEndAsync();

        //var jsonObject = JsonConvert.DeserializeObject<Dictionary<string, object>>(requestBody);
        //var ignoredKeys = new List<string>() { "password" };
        //foreach (var ignoredKey in ignoredKeys)
        //{
        //    jsonObject.Remove(ignoredKey);
        //}

        logger.LogInformation(
            $"Method: {context.Request.Method.ToUpper()} | Path: {context.Request.Path} | Status Code: {context.Response.StatusCode}" +
            $"{Environment.NewLine}Query String: {context.Request.QueryString}" +
            $"{Environment.NewLine}Request Body:" +
            $"{Environment.NewLine}{requestBody}"
        );
    }

    private async Task LogErrorCustomException(HttpContext context, Exception exception)
    {
        context.Request.Body.Seek(0, SeekOrigin.Begin);
        string requestBody = await new StreamReader(context.Request.Body).ReadToEndAsync();

        logger.LogError(
            $"Method: {context.Request.Method.ToUpper()} | Path: {context.Request.Path} | Status Code: {context.Response.StatusCode} | Message: {exception.Message}" +
            $"{Environment.NewLine}Query String: {context.Request.QueryString}" +
            $"{Environment.NewLine}Request Body:" +
            $"{Environment.NewLine}{requestBody}"
        );
    }

    private async Task LogErrorException(HttpContext context, Exception exception)
    {
        context.Request.Body.Seek(0, SeekOrigin.Begin);
        string requestBody = await new StreamReader(context.Request.Body).ReadToEndAsync();

        logger.LogError(
            $"Method: {context.Request.Method.ToUpper()} | Path: {context.Request.Path} | Status Code: {context.Response.StatusCode} | Message: {exception.Message}" +
            $"{Environment.NewLine}Query String: {context.Request.QueryString}" +
            $"{Environment.NewLine}Request Body:" +
            $"{Environment.NewLine}{requestBody}" +
            $"{Environment.NewLine}Raw:" +
            $"{Environment.NewLine}{exception}"
        );
    }
}