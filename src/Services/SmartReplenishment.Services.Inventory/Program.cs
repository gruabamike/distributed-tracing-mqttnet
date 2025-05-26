using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SmartReplenishment.Messaging.Mqtt;
using SmartReplenishment.Services.Inventory.BackgroundServices;
using SmartReplenishment.Services.Inventory.Data;

var builder = WebApplication.CreateBuilder(args);

// Mqtt Settings
IMqttSettings? mqttSettings = builder.Configuration
  .GetRequiredSection(IMqttSettings.MqttSettingsKey)
  .Get<IMqttSettings>();

ArgumentNullException.ThrowIfNull(mqttSettings);
mqttSettings.ClientId ??= builder.Environment.ApplicationName;

// Register Services
builder.Services.AddSingleton<IMqttSettings>(mqttSettings);

builder.AddServiceDefaults();

// Database
builder.Services.AddDbContext<InventoryDbContext>(options => options
  .UseNpgsql(builder.Configuration.GetConnectionString("InventoryDb")));

// Background Services
builder.Services.AddHostedService<Worker>();

var app = builder.Build();

// Database Seed
using (var scope = app.Services.CreateScope())
{
  var context = scope.ServiceProvider.GetRequiredService<InventoryDbContext>();
  context.Database.EnsureCreated();
  context.Database.Migrate();
  DatabaseSeeder.Seed(context);
}

app.MapPost("/stock/{productName}/increase", async (
  string productName, int amount, InventoryDbContext context) =>
{
  var stockProduct = await context.StockProducts
    .FirstOrDefaultAsync(p => p.Name == productName);

  if (stockProduct is null)
  {
    return Results.NotFound($"Product '{productName}' not found.");
  }

  stockProduct.Amount += amount;
  await context.SaveChangesAsync();

  return Results.Ok($"Stock for '{productName}' increased by {amount}.");
});

app.Run();
