using AspnetCore.Utilities.AppsettingVariables;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AspnetCore.Configuration.Logger;

public sealed class FileLoggerProvider : ILoggerProvider
{
    private readonly IServiceCollection _services;

    public FileLoggerProvider(IServiceCollection services)
    {
        _services = services;
    }

    public ILogger CreateLogger(string categoryName)
    {
        using var scope = _services.BuildServiceProvider().CreateScope();
        var environmentVariable = scope.ServiceProvider.GetRequiredService<IOptions<EnvironmentVariable>>();
        return new FileLoggerConfiguration(environmentVariable.Value.LoggingFolder);
    }

    public void Dispose()
    {
        throw new NotImplementedException();
    }
}

public class FileLoggerConfiguration : ILogger
{
    private readonly string _filePath;
    private static readonly object Lock = new();

    public FileLoggerConfiguration(string path)
    {
        _filePath = path;
    }

    public IDisposable BeginScope<TState>(TState state) where TState : notnull
    {
        return null;
    }

    public bool IsEnabled(LogLevel logLevel)
    {
        return true;
    }

    void ILogger.Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
    {
        if (formatter != null)
        {
            lock (Lock)
            {
                if (!Directory.Exists(_filePath))
                    Directory.CreateDirectory(_filePath);
                var fullFilePath = Path.Combine(_filePath, DateTime.Now.ToString("yyyy-MM-dd") + "_log.txt");
                var n = Environment.NewLine;
                var exc = "";
                if (exception != null)
                    exc = n + exception.GetType() + ": " + exception.Message + n + exception.StackTrace + n;
                File.AppendAllText(fullFilePath, logLevel + ": " + DateTime.Now + " " + formatter(state, exception) + n + exc);
            }
        }
    }
}