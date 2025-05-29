using System.Diagnostics.Tracing;

namespace SmartReplenishment.OTel.Instrumentation.MqttNetClientListener;

[EventSource(Name = "SmartReplenishment.OTel.Instrumentation")]
internal class InstrumentationEventSource : EventSource
{
  public static readonly InstrumentationEventSource Log = new InstrumentationEventSource();

  [Event(1, Message = "Current Activity is NULL in the '{0}' callback. Activity will not be recorded.", Level = EventLevel.Warning)]
  public void NullActivity(string eventName)
  {
    WriteEvent(1, eventName);
  }

  [NonEvent]
  public void UnknownErrorProcessingEvent(string handlerName, string eventName, Exception ex)
  {
    if (IsEnabled(EventLevel.Error, EventKeywords.All))
    {
      UnknownErrorProcessingEvent(handlerName, eventName, ex);
    }
  }

  [Event(2, Message = "Unknown error processing event '{1}' from handler '{0}', Exception: {2}", Level = EventLevel.Error)]
  public void UnknownErrorProcessingEvent(string handlerName, string eventName, string ex)
  {
    WriteEvent(2, handlerName, eventName, ex);
  }

  [Event(3, Message = "Payload is NULL in event '{1}' from handler '{0}', span will not be recorded.", Level = EventLevel.Warning)]
  public void NullPayload(string handlerName, string eventName)
  {
    this.WriteEvent(3, handlerName, eventName);
  }

  [Event(4, Message = "Payload is invalid in event '{1}' from handler '{0}', span will not be recorded.", Level = EventLevel.Warning)]
  public void InvalidPayload(string handlerName, string eventName)
  {
    this.WriteEvent(4, handlerName, eventName);
  }
}
