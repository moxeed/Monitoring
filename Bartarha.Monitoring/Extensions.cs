using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpLogging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Instrumentation.AspNetCore;
using OpenTelemetry.Instrumentation.Http;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using TestAttribute;

namespace Bartarha.Monitoring;

public static class Extensions
{
    private static void EnrichHttpClient(HttpClientInstrumentationOptions options)
    {
        options.Enrich = (activity, eventName, rawObject) =>
        {
            if (eventName.Equals("OnStartActivity"))
            {
                if (rawObject is HttpRequestMessage httpRequest)
                {
                    var request = "empty";
                    if (httpRequest.Content != null)
                        request = httpRequest.Content.ReadAsStringAsync().Result;
                    activity.SetTag("http.request_content", request);
                }
            }

            if (eventName.Equals("OnStopActivity"))
            {
                if (rawObject is HttpResponseMessage httpResponse)
                {
                    var response = httpResponse.Content.ReadAsStringAsync().Result;
                    activity.SetTag("http.response_content", response);
                }
            }
        };
    }

    private static void EnrichAspNetCore(AspNetCoreInstrumentationOptions options)
    {
        options.Enrich = (activity, eventName, rawObject) =>
        {
            if (eventName.Equals("OnStartActivity") && rawObject is HttpRequest httpRequest)
            {
                httpRequest.EnableBuffering();
                var request = new StreamReader(httpRequest.Body).ReadToEndAsync().Result;
                activity.SetTag("http.request_content", request);
                httpRequest.Body.Position = 0;
            }

            if (eventName.Equals("OnStopActivity") && rawObject is HttpResponse httpResponse)
            {
                var response = new StreamReader(httpResponse.Body).ReadToEndAsync().Result;
                activity.SetTag("http.response_content", response);
            }
        };
    }

    public static void AddMonitoring(this IServiceCollection services, string serviceName)
    {
        services.AddOpenTelemetryTracing(builder =>
            builder
                .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService(serviceName))
                .AddSource(Span.BartarhaSource)
                .AddAspNetCoreInstrumentation()
                .AddHttpClientInstrumentation()
                .AddSqlClientInstrumentation()
                .AddJaegerExporter());

        services.AddHttpLogging(options =>
        {
            options.LoggingFields = HttpLoggingFields.RequestHeaders |
                                    HttpLoggingFields.RequestBody |
                                    HttpLoggingFields.ResponseHeaders |
                                    HttpLoggingFields.ResponseBody;
        });

        services.AddSingleton(typeof(ILogger<>), typeof(TraceLogger<>));
    }

    public static void UseMonitoring(this WebApplication app)
    {
        app.UseHttpLogging();
    }
}