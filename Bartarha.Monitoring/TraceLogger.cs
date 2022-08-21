using System.Diagnostics;
using Microsoft.Extensions.Logging;
using TestAttribute;

namespace Bartarha.Monitoring;

public class TraceLogger<T> : ILogger<T>, IDisposable
{
    public IDisposable BeginScope<TState>(TState state)
    {
        return this;
    }

    public bool IsEnabled(LogLevel logLevel)
    {
        if (typeof(T).Namespace == "Microsoft.AspNetCore.HttpLogging")
            return true;
        return logLevel > LogLevel.Information;
    }

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        var activity = Activity.Current;
        if (activity is null)
        {
            LogInNewSpan(logLevel, eventId, state, exception, formatter);
        }
        else
        {
            LogInCurrentSpan(logLevel, eventId, state, exception, formatter);   
        }
    }

    private void LogInNewSpan<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception,
        Func<TState, Exception?, string> formatter)
    {
        using var span = new Span("Logging");
        LogInCurrentSpan(logLevel, eventId, state, exception, formatter);
    }

    private void LogInCurrentSpan<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception,
        Func<TState, Exception?, string> formatter)
    {
        var activity = Activity.Current;
        activity?.SetTag(eventId.Name ?? logLevel.ToString() , formatter(state, exception));
        activity?.SetTag(nameof(logLevel), logLevel.ToString());
    }

    public void Dispose()
    {
    }
}