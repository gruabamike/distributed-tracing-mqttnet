using MQTTnet;

namespace SmartReplenishment.OTel.Instrumentation.MqttNetClientDecorator;

internal sealed class MqttClientArgumentNullException : ArgumentNullException
{
  public MqttClientArgumentNullException(string paramName)
    : base(paramName, $"Service of type {nameof(IMqttClient)} must be supplied when dependency injection is unavailable. " +
                             "Use OpenTelemetry.Extensions.Hosting package to enable dependency injection.")
  {
  }
}
