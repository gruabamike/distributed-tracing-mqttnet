using SmartReplenishment.Shared.Model;

namespace SmartReplenishment.Services.Inventory.Data;

public static class DatabaseSeeder
{
  private static readonly StockArticle _stockArticleA = new() { Name = "Article A", Amount = 100 };
  private static readonly StockArticle _stockArticleB = new() { Name = "Article B", Amount = 125 };
  private static readonly StockArticle _stockArticleC = new() { Name = "Article C", Amount = 150 };

  private static readonly StockArticleConfiguration _stockArticleConfigurationA = new() { StockArticle = _stockArticleA, MinStockThreshold = 20 };
  private static readonly StockArticleConfiguration _stockArticleConfigurationB = new() { StockArticle = _stockArticleB, MinStockThreshold = 30 };
  private static readonly StockArticleConfiguration _stockArticleConfigurationC = new() { StockArticle = _stockArticleC, MinStockThreshold = 40 };

  public static void Seed(InventoryDbContext context)
  {
    if (!context.StockArticles.Any())
    {
      context.StockArticles.AddRange(_stockArticleA, _stockArticleB, _stockArticleC);
      context.SaveChanges();
    }
    if (!context.StockArticleConfigurations.Any())
    {
      context.StockArticleConfigurations.AddRange(_stockArticleConfigurationA, _stockArticleConfigurationB, _stockArticleConfigurationC);
      context.SaveChanges();
    }
  }
}
