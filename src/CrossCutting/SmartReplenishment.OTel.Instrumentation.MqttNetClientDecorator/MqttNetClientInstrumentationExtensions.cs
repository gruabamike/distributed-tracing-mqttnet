using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using MQTTnet;

namespace SmartReplenishment.OTel.Instrumentation.MqttNetClientDecorator;

public static class MqttNetClientInstrumentationExtensions
{
  public static IServiceCollection AddMqttNetClientDecoratorTracing(this IServiceCollection services)
    => services.AddMqttNetClientDecoratorTracing(new MqttClientFactory());

  public static IServiceCollection AddMqttNetClientDecoratorTracing(this IServiceCollection services, MqttClientFactory mqttClientFactory)
  {
    ArgumentNullException.ThrowIfNull(services);
    ArgumentNullException.ThrowIfNull(mqttClientFactory);
    services.Replace(ServiceDescriptor.Singleton<IMqttClient>(sp =>
      new MqttNetClientDecoratorTracing(mqttClientFactory.CreateMqttClient())
    ));
    return services;
  }
}
