using MQTTnet;
using MQTTnet.Packets;
using OpenTelemetry.Trace;
using System.Diagnostics;

namespace SmartReplenishment.OTel.Instrumentation.MqttNetClientListener;

internal sealed class DiagnosticSourceListenerMqttNetClient : DiagnosticSourceListenerBase
{
  internal static readonly string ActivitySourceName = typeof(DiagnosticSourceListenerMqttNetClient).Assembly.GetName().Name!;

  internal const string DiagnosticSourceName = "MQTTnet.MqttClient";

  internal const string EventNamePrefix = DiagnosticSourceName;
  public const string EventNamePublishStart = $"{EventNamePrefix}.Publish.Start";
  public const string EventNamePublishStop = $"{EventNamePrefix}.Publish.Stop";
  public const string EventNamePublishError = $"{EventNamePrefix}.Publish.Exception";
  public const string EventNameConsumeStart = $"{EventNamePrefix}.Consume.Start";
  public const string EventNameConsumeStop = $"{EventNamePrefix}.Consume.Stop";
  public const string EventNameConsumeError = $"{EventNamePrefix}.Consume.Exception";

  public override bool SupportsNullActivity => true;

  public DiagnosticSourceListenerMqttNetClient(string sourceName) : base(sourceName)
  {
  }

  public override void OnStartActivity(string name, Activity? activity, object? payload)
  {
    switch (name)
    {
      case EventNamePublishStart:
        {
          if (
            !AnonymousObjectPropertyFetcher.TryGetProperty(payload, "ApplicationMessage", out MqttApplicationMessage? applicationMessage) ||
            !AnonymousObjectPropertyFetcher.TryGetProperty(payload, "Options", out MqttClientOptions? mqttClientOptions))
          {
            InstrumentationEventSource.Log.NullPayload(nameof(DiagnosticSourceListenerMqttNetClient), name);
            return;
          }

          activity = MqttNetClientInstrumentationSource.ActivitySource.StartActivity(
            MqttNetClientActivityHelper.ActivityNamePublish(applicationMessage.Topic),
            ActivityKind.Producer,
            default(ActivityContext),
            MqttNetClientActivityHelper.PublishTags(applicationMessage, mqttClientOptions));

          if (activity is null)
            return; // there is no listener or the sampler decided not to sample the current trace

          applicationMessage.UserProperties ??= new List<MqttUserProperty>();
          MqttNetClientContextPropagationHandler.Inject(activity.Context, applicationMessage.UserProperties);

          if (activity.IsAllDataRequested)
            MqttNetClientActivityHelper.AddAdditionalTags(activity, applicationMessage, mqttClientOptions);
        }
        break;
      case EventNameConsumeStart:
        {
          if (
            !AnonymousObjectPropertyFetcher.TryGetProperty(payload, "ApplicationMessage", out MqttApplicationMessage? applicationMessage) ||
            !AnonymousObjectPropertyFetcher.TryGetProperty(payload, "Options", out MqttClientOptions? mqttClientOptions))
          {
            InstrumentationEventSource.Log.NullPayload(nameof(DiagnosticSourceListenerMqttNetClient), name);
            return;
          }

          var parentContext = MqttNetClientContextPropagationHandler.Extract(applicationMessage.UserProperties);
          activity = MqttNetClientInstrumentationSource.ActivitySource.StartActivity(
            MqttNetClientActivityHelper.ActivityNameConsume(applicationMessage.Topic),
            ActivityKind.Consumer,
            parentContext.ActivityContext,
            MqttNetClientActivityHelper.ConsumeTags(applicationMessage, mqttClientOptions));

          if (activity is null)
            return; // there is no listener or the sampler decided not to sample the current trace

          if (activity.IsAllDataRequested)
            MqttNetClientActivityHelper.AddAdditionalTags(activity, applicationMessage, mqttClientOptions);
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

          if (activity.Source != MqttNetClientInstrumentationSource.ActivitySource)
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

          if (activity.Source != MqttNetClientInstrumentationSource.ActivitySource)
            return;

          try
          {
            if (activity.IsAllDataRequested)
            {
              if (AnonymousObjectPropertyFetcher.TryGetProperty(payload, "Exception", out Exception? exception))
                activity.SetStatus(ActivityStatusCode.Error, exception.Message);
              else
                InstrumentationEventSource.Log.NullPayload(nameof(DiagnosticSourceListenerMqttNetClient), name);
            }
          }
          finally { activity.Stop(); }
        }
        break;
    }
  }

  public override void OnCustom(string name, Activity? activity, object? payload)
  {
    
  }
}
