using MQTTnet;
using MQTTnet.Packets;
using System.Diagnostics;

namespace SmartReplenishment.OTel.Instrumentation.MqttNetClientDecorator;

public sealed class MqttNetClientDecoratorTracing : MqttNetClientDecorator
{
  public MqttNetClientDecoratorTracing(IMqttClient mqttClient) : base(mqttClient) { }

  protected override Task OnApplicationMessageReceivedAsync(MqttApplicationMessageReceivedEventArgs e)
  {
    var parentContext = MqttNetClientContextPropagationHandler.Extract(e.ApplicationMessage.UserProperties);
    using var activity = MqttNetClientInstrumentationSource.ActivitySource.StartActivity(
      MqttNetClientActivityHelper.ActivityNameConsume(e.ApplicationMessage.Topic),
      ActivityKind.Consumer,
      parentContext.ActivityContext,
      MqttNetClientActivityHelper.ConsumeTags(e.ApplicationMessage, Options));

    if (activity != null && activity.IsAllDataRequested)
      MqttNetClientActivityHelper.AddAdditionalTags(activity, e.ApplicationMessage, Options);

    return base.OnApplicationMessageReceivedAsync(e);
  }

  public override Task<MqttClientPublishResult> PublishAsync(MqttApplicationMessage applicationMessage, CancellationToken cancellationToken = default)
  {
    using var activity = MqttNetClientInstrumentationSource.ActivitySource.StartActivity(
      MqttNetClientActivityHelper.ActivityNamePublish(applicationMessage.Topic),
      ActivityKind.Producer,
      default(ActivityContext),
      MqttNetClientActivityHelper.PublishTags(applicationMessage, Options));

    if (activity != null)
    {
      applicationMessage.UserProperties ??= new List<MqttUserProperty>();
      MqttNetClientContextPropagationHandler.Inject(activity.Context, applicationMessage.UserProperties);

      if (activity.IsAllDataRequested)
        MqttNetClientActivityHelper.AddAdditionalTags(activity, applicationMessage, Options);
    }

    return base.PublishAsync(applicationMessage, cancellationToken);
  }
}
