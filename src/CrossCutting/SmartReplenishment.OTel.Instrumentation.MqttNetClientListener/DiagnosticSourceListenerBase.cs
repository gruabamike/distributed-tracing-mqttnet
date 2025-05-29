using System.Diagnostics;

namespace SmartReplenishment.OTel.Instrumentation.MqttNetClientListener;

internal abstract class DiagnosticSourceListenerBase
{
  private readonly string _sourceName;

  protected DiagnosticSourceListenerBase(string sourceName)
  {
    _sourceName = sourceName ?? throw new ArgumentNullException(nameof(sourceName));
  }

  public string SourceName => _sourceName;

  public virtual bool SupportsNullActivity { get; }

  public virtual void OnStartActivity(Activity? activity, object? payload) { }

  public virtual void OnStopActivity(Activity? activity, object? payload) { }

  public virtual void OnException(Activity? activity, object? payload) { }

  public virtual void OnCustom(string name, Activity? activity, object? payload) { }
}
