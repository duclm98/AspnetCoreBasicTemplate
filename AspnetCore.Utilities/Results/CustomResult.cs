using AspnetCore.Utilities.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections;
using System.Text.Json;

namespace AspnetCore.Utilities.Results;

public class CustomResult : ActionResult
{
    private readonly int _httpStatus;
    private readonly string _message;
    private readonly object _data;

    public CustomResult(string message, object data, int httpStatus = 200)
    {
        _httpStatus = httpStatus;
        _message = message;
        _data = data;
    }

    public override async Task ExecuteResultAsync(ActionContext context)
    {
        context.HttpContext.Response.ContentType = "application/json";
        context.HttpContext.Response.StatusCode = _httpStatus;

        string resultDetail;
        if (_data is PaginationResult result && result.Data is IList data)
        {
            resultDetail = new ResultDetail
            {
                Message = _message,
                StatusCode = _httpStatus,
                Data = new PaginationResult(new BaseFilteringModel
                {
                    PageCount = result.PageCount > 0 ? result.PageCount : data.Count,
                    Page = result.Page > 0 ? result.Page : 1
                }, result.TotalRecord, data)
            }.ToString();
        }
        else
        {
            resultDetail = new ResultDetail()
            {
                Message = _message,
                StatusCode = _httpStatus,
                Data = _data
            }.ToString();
        }

        await context.HttpContext.Response.WriteAsync(resultDetail);
    }
}

public class PaginationResult
{
    public int TotalRecord { get; set; }
    public int TotalPages { get; set; }
    public int PageCount { get; set; }
    public int Page { get; set; }
    public int TotalInPage { get; set; }
    public object Data { get; set; }

    public PaginationResult() { }

    public PaginationResult(BaseFilteringModel filter, int totalRecord, object data)
    {
        if (data is IList dataList)
        {
            totalRecord = totalRecord > 0 ? totalRecord : dataList.Count;
            var pageCount = filter.PageCount > 0 ? filter.PageCount : dataList.Count;
            var page = filter.Page > 0 ? filter.Page : 1;

            TotalRecord = totalRecord;
            TotalPages = (int)Math.Ceiling(totalRecord / (double)filter.PageCount);
            PageCount = pageCount;
            Page = page;
            TotalInPage = dataList.Count;
            Data = dataList;
        }
    }

    public PaginationResult(BaseFilteringModel filter, int totalRecord, IList dataList)
    {
        totalRecord = totalRecord > 0 ? totalRecord : dataList.Count;
        var pageCount = filter.PageCount > 0 ? filter.PageCount : dataList.Count;
        var page = filter.Page > 0 ? filter.Page : 1;

        TotalRecord = totalRecord;
        TotalPages = (int)Math.Ceiling(totalRecord / (double)filter.PageCount);
        PageCount = pageCount;
        Page = page;
        TotalInPage = dataList.Count;
        Data = dataList;
    }
}

public class ResultDetail
{
    public int StatusCode { get; set; }
    public string Message { get; set; }
    public object Data { get; set; }

    public override string ToString()
    {
        if (Data == null)
            return JsonSerializer.Serialize(new
            {
                statusCode = StatusCode,
                message = Message
            });

        return JsonSerializer.Serialize(new
        {
            statusCode = StatusCode,
            message = Message,
            data = Data
        }, new JsonSerializerOptions()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });
    }
}