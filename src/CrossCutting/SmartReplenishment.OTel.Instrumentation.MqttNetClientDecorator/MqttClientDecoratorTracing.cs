using MQTTnet;
using MQTTnet.Packets;
using System.Diagnostics;

namespace SmartReplenishment.OTel.Instrumentation.MqttNetClientDecorator;

public sealed class MqttClientDecoratorTracing : MqttClientDecorator
{
  public MqttClientDecoratorTracing(IMqttClient mqttClient) : base(mqttClient) { }

  protected override Task OnApplicationMessageReceivedAsync(MqttApplicationMessageReceivedEventArgs e)
  {
    var parentContext = MqttClientContextPropagationHandler.Extract(e.ApplicationMessage.UserProperties);
    using var activity = MqttClientActivitySourceProvider.ActivitySource.StartActivity(
      MqttClientActivityHelper.GetActivityNameConsume(e.ApplicationMessage.Topic),
      ActivityKind.Consumer,
      parentContext.ActivityContext,
      MqttClientActivityHelper.ConsumeTags(e.ApplicationMessage, Options));

    if (activity != null && activity.IsAllDataRequested)
      MqttClientActivityHelper.AddAdditionalTags(activity, e.ApplicationMessage, Options);

    return base.OnApplicationMessageReceivedAsync(e);
  }

  public override Task<MqttClientPublishResult> PublishAsync(MqttApplicationMessage applicationMessage, CancellationToken cancellationToken = default)
  {
    using var activity = MqttClientActivitySourceProvider.ActivitySource.StartActivity(
      MqttClientActivityHelper.GetActivityNamePublish(applicationMessage.Topic),
      ActivityKind.Producer,
      default(ActivityContext),
      MqttClientActivityHelper.PublishTags(applicationMessage, Options));

    if (activity != null)
    {
      applicationMessage.UserProperties ??= new List<MqttUserProperty>();
      MqttClientContextPropagationHandler.Inject(activity.Context, applicationMessage.UserProperties);

      if (activity.IsAllDataRequested)
        MqttClientActivityHelper.AddAdditionalTags(activity, applicationMessage, Options);
    }

    return base.PublishAsync(applicationMessage, cancellationToken);
  }
}
