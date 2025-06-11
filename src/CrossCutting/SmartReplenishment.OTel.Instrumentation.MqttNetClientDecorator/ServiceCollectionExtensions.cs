using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using MQTTnet;

namespace SmartReplenishment.OTel.Instrumentation.MqttNetClientDecorator;

public static class ServiceCollectionExtensions
{
  public static IServiceCollection AddMqttNetClientDecoratorTracing(this IServiceCollection services)
    => services.AddMqttNetClientDecoratorTracing(() => new MqttClientFactory().CreateMqttClient());

  public static IServiceCollection AddMqttNetClientDecoratorTracing(this IServiceCollection services, Func<IMqttClient> mqttClientProvider)
  {
    ArgumentNullException.ThrowIfNull(services);
    ArgumentNullException.ThrowIfNull(mqttClientProvider);

    return services.Replace(ServiceDescriptor.Singleton<IMqttClient>(sp =>
      new MqttClientDecoratorTracing(mqttClientProvider())
    ));
  }
}
