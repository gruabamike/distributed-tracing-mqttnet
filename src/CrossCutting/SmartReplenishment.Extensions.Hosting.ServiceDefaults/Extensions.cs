using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.ServiceDiscovery;
using MQTTnet;
using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

#if MQTT_INSTRUMENTATION_TYPE_DECORATOR
using SmartReplenishment.OTel.Instrumentation.MqttNetClientDecorator;
#elif MQTT_INSTRUMENTATION_TYPE_ACTIVITYSOURCE
using SmartReplenishment.OTel.Instrumentation.MqttNetClientActivity;
#elif MQTT_INSTRUMENTATION_TYPE_DIAGNOSTICLISTENER
using SmartReplenishment.OTel.Instrumentation.MqttNetClientListener;
#endif

namespace Microsoft.Extensions.Hosting;
// Adds common .NET Aspire services: service discovery, resilience, health checks, and OpenTelemetry.
// This project should be referenced by each service project in your solution.
// To learn more about using this project, see https://aka.ms/dotnet/aspire/service-defaults
public static class Extensions
{
  public static TBuilder AddServiceDefaults<TBuilder>(this TBuilder builder) where TBuilder : IHostApplicationBuilder
  {
    // Register IMqttClient as singleton
#if MQTT_INSTRUMENTATION_TYPE_DECORATOR
    builder.Services.AddMqttNetClientDecoratorTracing();
#elif MQTT_INSTRUMENTATION_TYPE_ACTIVITYSOURCE || MQTT_INSTRUMENTATION_TYPE_DIAGNOSTICLISTENER
    builder.Services.AddSingleton<IMqttClient>(sp => new MqttClientFactory().CreateMqttClient());
#endif

    builder.ConfigureOpenTelemetry();

    builder.AddDefaultHealthChecks();

    builder.Services.AddServiceDiscovery();

    builder.Services.ConfigureHttpClientDefaults(http =>
    {
      // Turn on resilience by default
      http.AddStandardResilienceHandler();

      // Turn on service discovery by default
      http.AddServiceDiscovery();
    });

    // Uncomment the following to restrict the allowed schemes for service discovery.
    // builder.Services.Configure<ServiceDiscoveryOptions>(options =>
    // {
    //     options.AllowedSchemes = ["https"];
    // });

    return builder;
  }

  public static TBuilder ConfigureOpenTelemetry<TBuilder>(this TBuilder builder) where TBuilder : IHostApplicationBuilder
  {
    builder.Logging.AddOpenTelemetry(logging =>
    {
      logging.IncludeFormattedMessage = true;
      logging.IncludeScopes = true;
    });

    builder.Services.AddOpenTelemetry()
      .ConfigureResource(resource => resource.AddEnvironmentVariableDetector())
      .WithMetrics(metrics =>
      {
        metrics
          .AddAspNetCoreInstrumentation()
          .AddHttpClientInstrumentation()
          .AddRuntimeInstrumentation();
      })
      .WithTracing(tracing =>
      {
        tracing
          .AddSource(builder.Environment.ApplicationName)
          .AddAspNetCoreInstrumentation()
          // Uncomment the following line to enable gRPC instrumentation (requires the OpenTelemetry.Instrumentation.GrpcNetClient package)
          //.AddGrpcClientInstrumentation()
          .AddHttpClientInstrumentation()
          .AddEntityFrameworkCoreInstrumentation(options =>
          {
            options.SetDbStatementForText = true;
            options.SetDbStatementForStoredProcedure = true;
          })
          .AddMqttNetClientInstrumentation();

      });

    builder.AddOpenTelemetryExporters();

    return builder;
  }

  private static TBuilder AddOpenTelemetryExporters<TBuilder>(this TBuilder builder) where TBuilder : IHostApplicationBuilder
  {
    var useOtlpExporter = !string.IsNullOrWhiteSpace(builder.Configuration["OTEL_EXPORTER_OTLP_ENDPOINT"]);

    if (useOtlpExporter)
    {
      builder.Services.AddOpenTelemetry().UseOtlpExporter();
    }

    return builder;
  }

  public static TBuilder AddDefaultHealthChecks<TBuilder>(this TBuilder builder) where TBuilder : IHostApplicationBuilder
  {
    builder.Services.AddHealthChecks()
        // Add a default liveness check to ensure app is responsive
        .AddCheck("self", () => HealthCheckResult.Healthy(), ["live"]);

    return builder;
  }

  public static WebApplication MapDefaultEndpoints(this WebApplication app)
  {
    // Adding health checks endpoints to applications in non-development environments has security implications.
    // See https://aka.ms/dotnet/aspire/healthchecks for details before enabling these endpoints in non-development environments.
    if (app.Environment.IsDevelopment())
    {
      // All health checks must pass for app to be considered ready to accept traffic after starting
      app.MapHealthChecks("/health");

      // Only health checks tagged with the "live" tag must pass for app to be considered alive
      app.MapHealthChecks("/alive", new HealthCheckOptions
      {
        Predicate = r => r.Tags.Contains("live")
      });
    }

    return app;
  }
}
