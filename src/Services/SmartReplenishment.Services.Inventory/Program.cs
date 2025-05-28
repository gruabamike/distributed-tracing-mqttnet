using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartReplenishment.Messaging.Http;
using SmartReplenishment.Messaging.Mqtt;
using SmartReplenishment.Services.Inventory.BackgroundServices;
using SmartReplenishment.Services.Inventory.Data;

var builder = WebApplication.CreateBuilder(args);

// Mqtt Settings
IMqttSettings? mqttSettings = builder.Configuration
  .GetRequiredSection(IMqttSettings.MqttSettingsKey)
  .Get<MqttSettings>();

ArgumentNullException.ThrowIfNull(mqttSettings);

// Register Services
builder.Services.AddSingleton<IMqttSettings>(mqttSettings);
builder.AddServiceDefaults();

// Register Database Context
builder.Services.AddDbContext<InventoryDbContext>(options => options
  .UseNpgsql(builder.Configuration.GetConnectionString("InventoryDb")));

// Background Services
builder.Services.AddHostedService<Worker>();

var app = builder.Build();

// Seed Database
using (var scope = app.Services.CreateScope())
{
  var context = scope.ServiceProvider.GetRequiredService<InventoryDbContext>();
  context.Database.EnsureCreated();
  context.Database.Migrate();
  DatabaseSeeder.Seed(context);
}

app.MapPut("/stock/{articleId}/increase", async (
  Guid articleId,
  [FromBody] StockAmountIncrease stockAmountIncrease,
  InventoryDbContext context) =>
{
  var stockArticle = await context.StockArticles
    .FirstOrDefaultAsync(p => p.Id == articleId);

  if (stockArticle is null)
  {
    return Results.NotFound($"Article '{articleId}' not found.");
  }

  stockArticle.Amount += stockAmountIncrease.Amount;
  await context.SaveChangesAsync();

  return Results.Ok($"Stock for '{articleId}' increased by {stockAmountIncrease.Amount}.");
});

app.Run();
