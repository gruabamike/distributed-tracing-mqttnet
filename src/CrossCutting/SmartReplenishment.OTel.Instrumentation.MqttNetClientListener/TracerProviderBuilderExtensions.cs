using OpenTelemetry.Trace;

namespace SmartReplenishment.OTel.Instrumentation.MqttNetClientListener;

public static class TracerProviderBuilderExtensions
{
  public static TracerProviderBuilder AddMqttNetClientInstrumentation(
    this TracerProviderBuilder builder,
    Action<MqttNetClientInstrumentationOptions>? configureOptions = null)
  {
    ArgumentNullException.ThrowIfNull(builder);

    var options = new MqttNetClientInstrumentationOptions();
    configureOptions?.Invoke(options);

    builder.AddInstrumentation(() => new MqttNetClientInstrumentation(options));
    builder.AddSource(DiagnosticSourceListenerMqttNetClient.ActivitySourceName);
    return builder;
  }
}
