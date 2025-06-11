using MQTTnet;

namespace SmartReplenishment.OTel.Instrumentation.MqttNetClientDecorator;

internal sealed class MqttClientNotResolvedInvalidOperationException : InvalidOperationException
{
  public MqttClientNotResolvedInvalidOperationException()
        : base($"Service of type {nameof(IMqttClient)} could not be resolved through application {nameof(IServiceProvider)}.")
  { 
  }
}
