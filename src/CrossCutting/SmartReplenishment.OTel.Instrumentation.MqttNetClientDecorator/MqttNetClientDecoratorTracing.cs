using MQTTnet;
using MQTTnet.Packets;
using OpenTelemetry;
using OpenTelemetry.Context.Propagation;
using System.Diagnostics;

namespace SmartReplenishment.OTel.Instrumentation.MqttNetClientDecorator;

public sealed class MqttNetClientDecoratorTracing : MqttNetClientDecorator
{
  private static readonly ActivitySource ActivitySource = MqttNetClientActivityHelper.ActivitySource;

  public MqttNetClientDecoratorTracing(IMqttClient mqttClient) : base(mqttClient)
  {
  }

  protected override Task OnApplicationMessageReceivedAsync(MqttApplicationMessageReceivedEventArgs e)
  {
    var parentContext = MqttNetClientContextPropagation.Propagator.Extract(
      default,
      e.ApplicationMessage.UserProperties,
      MqttNetClientContextPropagation.Extract);
    Baggage.Current = parentContext.Baggage;

    using var activity = ActivitySource.StartActivity("MQTT Receive", ActivityKind.Consumer, parentContext.ActivityContext);

    activity?.SetTag("mqtt.topic", e.ApplicationMessage.Topic);
    activity?.SetTag("mqtt.client_id", e.ClientId);
    activity?.SetTag("mqtt.payload_size", e.ApplicationMessage.Payload.Length);

    return base.OnApplicationMessageReceivedAsync(e);
  }

  public override Task<MqttClientPublishResult> PublishAsync(MqttApplicationMessage applicationMessage, CancellationToken cancellationToken = default)
  {
    using var activity = ActivitySource.StartActivity("MQTT Publish", ActivityKind.Producer);
    if (activity != null)
    {
      applicationMessage.UserProperties ??= new List<MqttUserProperty>();
      MqttNetClientContextPropagation.Propagator.Inject(
        new PropagationContext(activity.Context, Baggage.Current),
        applicationMessage.UserProperties,
        MqttNetClientContextPropagation.Inject);
    }

    return base.PublishAsync(applicationMessage, cancellationToken);
  }
}
