using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using MQTTnet;

namespace SmartReplenishment.Observability.Instrumentation.MqttNetClient;

public static class MqttNetClientInstrumentationExtensions
{
  public static IServiceCollection AddMqttNetClientInstrumentation(this IServiceCollection services)
    => services.AddMqttNetClientInstrumentation(new MqttClientFactory());

  public static IServiceCollection AddMqttNetClientInstrumentation(this IServiceCollection services, MqttClientFactory mqttClientFactory)
  {
    ArgumentNullException.ThrowIfNull(services);
    ArgumentNullException.ThrowIfNull(mqttClientFactory);
    services.Replace(ServiceDescriptor.Singleton<IMqttClient>(sp =>
      new MqttNetClientDecoratorTracing(mqttClientFactory.CreateMqttClient())
    ));
    return services;
  }
}
