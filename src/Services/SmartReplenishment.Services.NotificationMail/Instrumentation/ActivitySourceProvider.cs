using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Reflection;

namespace SmartReplenishment.Services.NotificationMail.Instrumentation;

internal static class ActivitySourceProvider// : IDisposable
{
  private static readonly AssemblyName AssemblyName = typeof(ActivitySourceProvider).Assembly.GetName();

  private static readonly string ActivitySourceName = AssemblyName.Name!;
  private static readonly string? ActivitySourceVersion = AssemblyName.Version?.ToString();

  public static readonly ActivitySource ActivitySource = new(ActivitySourceName, ActivitySourceVersion);
  //private static readonly AssemblyName AssemblyName = typeof(InstrumentationSources).Assembly.GetName();

  //private static readonly string ActivitySourceName = AssemblyName.Name!;
  //private static readonly string? ActivitySourceVersion = AssemblyName.Version?.ToString();

  //public static readonly ActivitySource ActivitySource = new(ActivitySourceName, ActivitySourceVersion);

  //internal const string MeterName = $"{ServiceName}.Meter";

  //private static readonly ActivitySource _activitySource = new(ServiceName, ServiceVersion);
  //private static readonly Meter _meter = new(MeterName, ServiceVersion);
  //private static readonly Counter<long> _mailNotificationCounter =
  //  _meter.CreateCounter<long>(name: "mailnotification.count", description: "Number of Mails sent");

  //public ActivitySource ActivitySource => _activitySource;
  //public Counter<long> MailNotificationCounter => _mailNotificationCounter;

  //public void Dispose()
  //{
  //  _activitySource.Dispose();
  //  _meter.Dispose();
  //}
}
