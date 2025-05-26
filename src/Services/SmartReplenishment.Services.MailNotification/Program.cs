using SmartReplenishment.Services.MailNotification.BackgroundServices;

var builder = Host.CreateApplicationBuilder(args);

builder.Logging.ClearProviders();
builder.AddServiceDefaults();
builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();
