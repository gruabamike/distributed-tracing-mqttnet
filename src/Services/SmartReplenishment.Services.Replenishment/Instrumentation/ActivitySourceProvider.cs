using System.Diagnostics;
using System.Reflection;

namespace SmartReplenishment.Services.Replenishment.Instrumentation;

internal static class ActivitySourceProvider
{
  private static readonly AssemblyName AssemblyName = typeof(ActivitySourceProvider).Assembly.GetName();

  private static readonly string ActivitySourceName = AssemblyName.Name!;
  private static readonly string? ActivitySourceVersion = AssemblyName.Version?.ToString();

  public static readonly ActivitySource ActivitySource = new(ActivitySourceName, ActivitySourceVersion);
}
