using OpenTelemetry.Trace;

namespace SmartReplenishment.OTel.Instrumentation.MqttNetClientListener;

public static class TracerProviderBuilderExtensions
{
  public static TracerProviderBuilder AddMqttNetClientInstrumentation(
    this TracerProviderBuilder builder)
  {
    ArgumentNullException.ThrowIfNull(builder);

    builder.AddInstrumentation(() => new MqttNetClientInstrumentation());
    builder.AddSource(DiagnosticSourceListenerMqttNetClient.ActivitySourceName);
    return builder;
  }
}
