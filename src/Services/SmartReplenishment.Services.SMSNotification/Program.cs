using SmartReplenishment.Services.SMSNotification.BackgroundServices;

var builder = Host.CreateApplicationBuilder(args);
builder.AddServiceDefaults();
builder.Services.AddHostedService<Worker>();


var host = builder.Build();
host.Run();
