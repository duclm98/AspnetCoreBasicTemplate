using AspnetCore.Business.Middlewares;
using AspnetCore.Configuration;
using AspnetCore.Configuration.Logger;
using AspnetCore.Data;
using AspnetCore.Utilities;
using AspnetCore.Utilities.AppsettingVariables;
using AspnetCore.Utilities.Exceptions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Newtonsoft.Json;

var builder = WebApplication.CreateBuilder(args);

// Environment
var environment = builder.Environment;
var configuration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables()
    .Build();
builder.Services.Configure<EnvironmentVariable>(configuration.GetSection("EnvironmentVariable"));

// Add services to the container.
builder.Services.AddHttpContextAccessor();
builder.Services.AddMemoryCache();
builder.Services.AddControllers().AddNewtonsoftJson(options =>
    options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore
);

builder.Services.RegisterAllDependency();
builder.Services.RegisterDataContext(configuration.GetConnectionString("Connection"));

// Logging
builder.Services.AddLogging(logging =>
{
    logging.AddProvider(new FileLoggerProvider(builder.Services));
});

// ApiVersioning
builder.Services.AddApiVersioning(setup =>
{
    setup.DefaultApiVersion = new ApiVersion(1, 0);
    setup.AssumeDefaultVersionWhenUnspecified = true;
    setup.ReportApiVersions = true;
});
builder.Services.AddVersionedApiExplorer(setup =>
{
    setup.GroupNameFormat = "'v'VVV";
    setup.SubstituteApiVersionInUrl = true;
});

// Swagger
builder.Services.AddSwaggerGen();
builder.Services.ConfigureOptions<SwaggerOptionsConfiguration>();

var app = builder.Build();

app.Use(async (context, next) =>
{
    context.Request.EnableBuffering();
    context.Response.Headers.Add("Content-Type", "application/json; charset=utf-8");
    await next.Invoke();
});

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        var provider = app.Services.GetRequiredService<IApiVersionDescriptionProvider>();
        provider.ApiVersionDescriptions.ToList().ForEach(description =>
        {
            c.SwaggerEndpoint($"{description.GroupName}/swagger.json",
                description.GroupName.ToUpperInvariant());
        });
    });
}

app.UseCors(config =>
{
    config.AllowAnyOrigin();
    config.AllowAnyHeader();
    config.AllowAnyMethod();
});

app.UseMiddleware<CustomExceptionMiddleware>();
app.UseMiddleware<JwtMiddleware>();
//app.UseMiddleware<AuthenticationMiddleware>(); // Có thể thêm hoặc không nếu muốn kiểm tra user trong database (sẽ làm giảm hiệu năng)

app.UseAuthorization();

app.MapControllers();

app.Run();