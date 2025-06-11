using MQTTnet;
using MQTTnet.Packets;
using System.Diagnostics;

namespace SmartReplenishment.OTel.Instrumentation.MqttNetClientListener;

internal sealed class DiagnosticSourceListenerMqttClient : DiagnosticSourceListenerBase
{
  internal static readonly string ActivitySourceName = typeof(DiagnosticSourceListenerMqttClient).Assembly.GetName().Name!;

  internal const string DiagnosticSourceName = "MQTTnet.MqttClient";
  internal const string EventNamePrefix = DiagnosticSourceName;

  public const string EventNamePublishStart = $"{EventNamePrefix}.Publish.Start";
  public const string EventNamePublishStop = $"{EventNamePrefix}.Publish.Stop";
  public const string EventNamePublishError = $"{EventNamePrefix}.Publish.Exception";
  public const string EventNameConsumeStart = $"{EventNamePrefix}.Consume.Start";
  public const string EventNameConsumeStop = $"{EventNamePrefix}.Consume.Stop";
  public const string EventNameConsumeError = $"{EventNamePrefix}.Consume.Exception";

  public override bool SupportsNullActivity => true;

  public DiagnosticSourceListenerMqttClient(string sourceName) : base(sourceName)
  {
  }

  public override void OnStartActivity(string name, Activity? activity, object? payload)
  {
    switch (name)
    {
      case EventNamePublishStart:
        {
          if (
            !PropertyFetcher.TryGetProperty(payload, "ApplicationMessage", out MqttApplicationMessage? applicationMessage) ||
            !PropertyFetcher.TryGetProperty(payload, "Options", out MqttClientOptions? mqttClientOptions))
          {
            InstrumentationEventSource.Log.NullPayload(nameof(DiagnosticSourceListenerMqttClient), name);
            return;
          }

          activity = MqttClientActivitySourceProvider.ActivitySource.StartActivity(
            MqttClientActivityHelper.GetActivityNamePublish(applicationMessage.Topic),
            ActivityKind.Producer,
            default(ActivityContext),
            MqttClientActivityHelper.PublishTags(applicationMessage, mqttClientOptions));

          if (activity is null)
            return; // there is no listener or the sampler decided not to sample the current trace

          applicationMessage.UserProperties ??= new List<MqttUserProperty>();
          MqttClientContextPropagationHandler.Inject(activity.Context, applicationMessage.UserProperties);

          if (activity.IsAllDataRequested)
            MqttClientActivityHelper.AddAdditionalTags(activity, applicationMessage, mqttClientOptions);
        }
        break;
      case EventNameConsumeStart:
        {
          if (
            !PropertyFetcher.TryGetProperty(payload, "ApplicationMessage", out MqttApplicationMessage? applicationMessage) ||
            !PropertyFetcher.TryGetProperty(payload, "Options", out MqttClientOptions? mqttClientOptions))
          {
            InstrumentationEventSource.Log.NullPayload(nameof(DiagnosticSourceListenerMqttClient), name);
            return;
          }

          var parentContext = MqttClientContextPropagationHandler.Extract(applicationMessage.UserProperties);
          activity = MqttClientActivitySourceProvider.ActivitySource.StartActivity(
            MqttClientActivityHelper.GetActivityNameConsume(applicationMessage.Topic),
            ActivityKind.Consumer,
            parentContext.ActivityContext,
            MqttClientActivityHelper.ConsumeTags(applicationMessage, mqttClientOptions));

          if (activity is null)
            return; // there is no listener or the sampler decided not to sample the current trace

          if (activity.IsAllDataRequested)
            MqttClientActivityHelper.AddAdditionalTags(activity, applicationMessage, mqttClientOptions);
        }
        break;
    }
  }

  public override void OnStopActivity(string name, Activity? activity, object? payload)
  {
    switch (name)
    {
      case EventNamePublishStop:
      case EventNameConsumeStop:
        {
          if (activity is null)
          {
            InstrumentationEventSource.Log.NullActivity(name);
            return;
          }

          if (activity.Source != MqttClientActivitySourceProvider.ActivitySource)
            return;

          try
          {
            if (activity.IsAllDataRequested)
              activity.SetStatus(ActivityStatusCode.Unset);
          }
          finally { activity.Stop(); }
        }
        break;
    }
  }

  public override void OnException(string name, Activity? activity, object? payload)
  {
    switch (name)
    {
        case EventNamePublishError:
        case EventNameConsumeError:
        {
          if (activity is null)
          {
            InstrumentationEventSource.Log.NullActivity(name);
            return;
          }

          if (activity.Source != MqttClientActivitySourceProvider.ActivitySource)
            return;

          try
          {
            if (activity.IsAllDataRequested)
            {
              if (PropertyFetcher.TryGetProperty(payload, "Exception", out Exception? exception))
                activity.SetStatus(ActivityStatusCode.Error, exception.Message);
              else
                InstrumentationEventSource.Log.NullPayload(nameof(DiagnosticSourceListenerMqttClient), name);
            }
          }
          finally { activity.Stop(); }
        }
        break;
    }
  }
}
