using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace SmartReplenishment.Services.MailNotification;

internal sealed class InstrumentationSources : IDisposable
{
  internal const string ServiceName = "SmartReplenishment.Services.MailNotification";
  internal const string ServiceVersion = "1.0.0";
  internal const string MeterName = $"{ServiceName}.Meter";

  private static readonly ActivitySource _activitySource = new(ServiceName, ServiceVersion);
  private static readonly Meter _meter = new(MeterName, ServiceVersion);
  private static readonly Counter<long> _mailNotificationCounter =
    _meter.CreateCounter<long>(name: "mailnotification.count", description: "Number of Mails sent");

  public ActivitySource ActivitySource => _activitySource;
  public Counter<long> MailNotificationCounter => _mailNotificationCounter;

  public void Dispose()
  {
    _activitySource.Dispose();
    _meter.Dispose();
  }
}
