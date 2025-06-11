using MQTTnet;

namespace SmartReplenishment.OTel.Instrumentation.MqttNetClientDecorator;

internal sealed class MqttClientNotDecoratedInvalidOperationException : InvalidOperationException
{
  public MqttClientNotDecoratedInvalidOperationException()
       : base($"Service of type {nameof(IMqttClient)} is not an instance of {nameof(MqttClientDecoratorTracing)}. " +
              $"Use {nameof(ServiceCollectionExtensions)} to register the corresponding decorator instance.")
  { 
  }
}
