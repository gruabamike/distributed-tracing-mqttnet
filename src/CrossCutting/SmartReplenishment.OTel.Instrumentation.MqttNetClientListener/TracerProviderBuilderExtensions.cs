using OpenTelemetry.Trace;

namespace SmartReplenishment.OTel.Instrumentation.MqttNetClientListener;

public static class TracerProviderBuilderExtensions
{
  public static TracerProviderBuilder AddMqttNetClientInstrumentation(
    this TracerProviderBuilder builder)
  {
    ArgumentNullException.ThrowIfNull(builder);

    builder.AddInstrumentation(() => new MqttClientInstrumentation());
    builder.AddSource(DiagnosticSourceListenerMqttClient.ActivitySourceName);
    return builder;
  }
}
