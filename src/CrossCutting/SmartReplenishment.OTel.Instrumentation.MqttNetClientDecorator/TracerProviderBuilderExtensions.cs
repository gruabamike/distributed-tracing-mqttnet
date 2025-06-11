using MQTTnet;
using OpenTelemetry.Trace;

namespace SmartReplenishment.OTel.Instrumentation.MqttNetClientDecorator;

public static class TracerProviderBuilderExtensions
{
  public static TracerProviderBuilder AddMqttNetClientInstrumentation(
    this TracerProviderBuilder builder,
    IMqttClient? mqttClient)
  {
    ArgumentNullException.ThrowIfNull(builder);

    if (builder is not IDeferredTracerProviderBuilder deferredTracerProviderBuilder)
    {
      if (mqttClient is null)
        throw new MqttClientArgumentNullException(nameof(mqttClient));
      if (mqttClient is not MqttClientDecoratorTracing)
        throw new MqttClientNotDecoratedInvalidOperationException();

      return builder.AddMqttNetClientInstrumentation();
    }

    return deferredTracerProviderBuilder.Configure((serviceProvider, builder) =>
    {
      if (mqttClient is null)
      {
        mqttClient = serviceProvider.GetService(typeof(IMqttClient)) as IMqttClient;
        if (mqttClient is null)
          throw new MqttClientNotResolvedInvalidOperationException();
      }

      if (mqttClient is not MqttClientDecoratorTracing)
        throw new MqttClientNotDecoratedInvalidOperationException();

      AddMqttNetClientInstrumentation(builder);
    });
  }

  private static TracerProviderBuilder AddMqttNetClientInstrumentation(
    this TracerProviderBuilder builder)
  {
    builder.AddSource(MqttClientActivitySourceProvider.ActivitySourceName);
    return builder;
  }
}
