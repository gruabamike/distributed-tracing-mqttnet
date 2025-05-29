using OpenTelemetry.Trace;

namespace SmartReplenishment.OTel.Instrumentation.MqttNetClientActivity;

public static class TracerProviderBuilderExtensions
{
  public static TracerProviderBuilder AddMqttNetClientInstrumentation(this TracerProviderBuilder builder)
  {
    ArgumentNullException.ThrowIfNull(builder);

    builder.AddSource(MqttNetClientActivitySourceProvider.ActivitySourceName);
    return builder;
  }
}
