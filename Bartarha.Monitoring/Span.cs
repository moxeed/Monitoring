using System.Diagnostics;

namespace TestAttribute;

public sealed class Span : IDisposable
{
    internal const string BartarhaSource = nameof(BartarhaSource);
    private static readonly ActivitySource ActivitySource = new(BartarhaSource);
    private readonly Activity? _activity;
    
    public Span(string name)
    {
        _activity = ActivitySource.StartActivity(name);
    }

    public void SetTag(string key, string value) => _activity?.SetTag(key, value);
    public void Dispose() => _activity?.Dispose();
}