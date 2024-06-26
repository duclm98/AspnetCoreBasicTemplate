using AspnetCore.Utilities.Models;
using AspnetCore.Utilities.Results;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AspnetCore.Api.Controllers;

[ApiController]
[AllowAnonymous]
[ApiVersion("1")]
[Route("v{version:apiVersion}/weather-forecasts")]
public class WeatherForecastController : ControllerBase
{
    private static readonly string[] Summaries = new[]
    {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching",
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching",
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching",
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching",
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching",
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching",
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching",
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching",
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching",
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching",
    };

    private readonly ILogger<WeatherForecastController> _logger;

    public WeatherForecastController(ILogger<WeatherForecastController> logger)
    {
        _logger = logger;
    }

    [HttpGet]
    public IActionResult GetWeatherForecasts([FromQuery] WeatherForecastFilteringModel filter)
    {
        var weatherForecasts = Summaries.Select(item => new WeatherForecast
        {
            TemperatureC = Random.Shared.Next(-20, 55),
            Summary = item
        })
        .ToList();
        var index = 0;
        foreach (var weatherForecast in weatherForecasts)
        {
            weatherForecast.Id = index + 1;
            weatherForecast.Date = DateTime.Now.AddDays(index);

            index++;
        }
        weatherForecasts = weatherForecasts.FindAll(x => string.IsNullOrEmpty(filter.SearchText) || x.Summary.ToLower().Contains(filter.SearchText.ToLower()));

        return new CustomResult("Thành công", new PaginationResult(filter, weatherForecasts.Count, weatherForecasts.Skip(filter.Skip()).Take(filter.PageCount).ToList()));
    }

    [HttpGet("{id}")]
    public IActionResult GetWeatherForecast(int id)
    {
        var weatherForecast = Enumerable.Range(1, 5).Select(index => new WeatherForecast
        {
            Id = index,
            Date = DateTime.Now.AddDays(index),
            TemperatureC = Random.Shared.Next(-20, 55),
            Summary = Summaries[Random.Shared.Next(Summaries.Length)]
        })
        .FirstOrDefault();
        return new CustomResult("Thành công", weatherForecast);
    }

    [HttpPost]
    public IActionResult CreateWeatherForecast([FromBody] WeatherForecast model)
    {
        return new CustomResult("Thành công", model);
    }

    [HttpPut("{id}")]
    public IActionResult UpdateWeatherForecast(int id, [FromBody] WeatherForecast model)
    {
        return new CustomResult("Thành công", model);
    }

    [HttpDelete("{id}")]
    public IActionResult DeleteWeatherForecast(int id)
    {
        return new CustomResult("Thành công", id);
    }
}

public class WeatherForecast
{
    public int Id { get; set; }
    public DateTime Date { get; set; }

    public int TemperatureC { get; set; }

    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);

    public string Summary { get; set; }
}

public class WeatherForecastFilteringModel : BaseFilteringModel
{
    public string SearchText { get; set; }
}