using Microsoft.Extensions.Options;
using MQTTnet;
using OpenTelemetry.Trace;

namespace SmartReplenishment.OTel.Instrumentation.MqttNetClientDecorator;

/// <summary>
/// Extension methods to simplify registering of dependency instrumentation.
/// </summary>
public static class TracerProviderBuilderExtensions
{
  public static TracerProviderBuilder AddMqttNetClientInstrumentation(
    this TracerProviderBuilder builder,
    IMqttClient? mqttClient = null,
    Action<MqttNetClientInstrumentationOptions>? configure = null)
  {
    ArgumentNullException.ThrowIfNull(builder);

    if (builder is not IDeferredTracerProviderBuilder deferredTracerProviderBuilder)
    {
      if (mqttClient is null)
        throw new InvalidOperationException($"Service of type {nameof(IMqttClient)} must be supplied when dependency injection is unavailable. Use OpenTelementry.Extensions.Hosting package to enable dependency injection.");
      if (mqttClient is not MqttNetClientDecoratorTracing)
        throw new InvalidOperationException($"Service of type .{nameof(IMqttClient)} is not an instance of {nameof(MqttNetClientDecoratorTracing)}. " +
          $"Use {nameof(MqttNetClientInstrumentationExtensions)} to register the corresponding decorator instance.");

      return builder.AddMqttNetClientInstrumentation(new MqttNetClientInstrumentationOptions(), configure);
    }

    return deferredTracerProviderBuilder.Configure((serviceProvider, builder) =>
    {
      if (mqttClient is null)
      {
        mqttClient = serviceProvider.GetService(typeof(IMqttClient)) as IMqttClient;
        if (mqttClient is null)
          throw new InvalidOperationException($"Service of type {nameof(IMqttClient)} could not be resolved through application {nameof(IServiceProvider)}.");
      
        if (mqttClient is not MqttNetClientDecoratorTracing)
          throw new InvalidOperationException($"Service of type {nameof(IMqttClient)} is not an instance of {nameof(MqttNetClientDecoratorTracing)}. " +
          $"Use {nameof(MqttNetClientInstrumentationExtensions)} to register the corresponding decorator instance.");
      }

      var optionService = serviceProvider.GetService(typeof(IOptions<MqttNetClientInstrumentationOptions>)) as IOptions<MqttNetClientInstrumentationOptions>;
      AddMqttNetClientInstrumentation(builder, optionService?.Value ?? new MqttNetClientInstrumentationOptions(), configure);
    });
  }

  /// <summary>
  /// Enables MQTTnet client instrumentation.
  /// </summary>
  /// <param name="builder"><see cref="TracerProviderBuilder"/> being configured.</param>
  /// <param name="configureOptions">MQTTnet client instrumentation configuration options.</param>
  /// <returns>The instance of <see cref="TracerProviderBuilder"/> to chain the calls.</returns>
  private static TracerProviderBuilder AddMqttNetClientInstrumentation(
    this TracerProviderBuilder builder,
    MqttNetClientInstrumentationOptions options,
    Action<MqttNetClientInstrumentationOptions>? configure = null)
  {
    configure?.Invoke(options);
    
    builder.AddSource(MqttNetClientActivityHelper.ActivitySourceName);
    return builder;
  }
}
