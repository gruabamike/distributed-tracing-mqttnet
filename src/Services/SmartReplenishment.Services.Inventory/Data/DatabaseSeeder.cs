using SmartReplenishment.Shared.Model;

namespace SmartReplenishment.Services.Inventory.Data;

public static class DatabaseSeeder
{
  private static readonly StockProduct _stockProductA = new() { Name = "Product A" };
  private static readonly StockProduct _stockProductB = new() { Name = "Product B" };
  private static readonly StockProduct _stockProductC = new() { Name = "Product C" };

  private static readonly StockConfiguration _stockConfigurationA = new() { StockProduct = _stockProductA, MinStockThreshold = 20 };
  private static readonly StockConfiguration _stockConfigurationB = new() { StockProduct = _stockProductB, MinStockThreshold = 30 };
  private static readonly StockConfiguration _stockConfigurationC = new() { StockProduct = _stockProductC, MinStockThreshold = 40 };

  public static void Seed(InventoryDbContext context)
  {
    if (!context.StockProducts.Any())
    {
      context.StockProducts.AddRange(_stockProductA, _stockProductB, _stockProductC);
      context.SaveChanges();
    }
    if (!context.StockConfigurations.Any())
    {
      context.StockConfigurations.AddRange(_stockConfigurationA, _stockConfigurationB, _stockConfigurationC);
      context.SaveChanges();
    }
  }
}
