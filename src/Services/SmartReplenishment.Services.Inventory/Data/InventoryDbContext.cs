using Microsoft.EntityFrameworkCore;
using SmartReplenishment.Shared.Model;

namespace SmartReplenishment.Services.Inventory.Data;

public class InventoryDbContext : DbContext
{
  public InventoryDbContext(DbContextOptions options) : base(options) { }

  public DbSet<StockArticle> StockArticles => Set<StockArticle>();
  public DbSet<StockArticleConfiguration> StockArticleConfigurations => Set<StockArticleConfiguration>();

  protected override void OnModelCreating(ModelBuilder modelBuilder)
  {
    modelBuilder.ApplyConfigurationsFromAssembly(typeof(EntityTypeConfigurationStockProduct).Assembly);
    base.OnModelCreating(modelBuilder);
  }
}
