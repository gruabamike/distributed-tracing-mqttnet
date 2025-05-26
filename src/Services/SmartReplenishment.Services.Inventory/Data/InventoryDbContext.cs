using Microsoft.EntityFrameworkCore;
using SmartReplenishment.Shared.Model;

namespace SmartReplenishment.Services.Inventory.Data;

public class InventoryDbContext : DbContext
{
  public InventoryDbContext(DbContextOptions options) : base(options) { }

  public DbSet<StockProduct> StockProducts => Set<StockProduct>();
  public DbSet<StockConfiguration> StockConfigurations => Set<StockConfiguration>();

  protected override void OnModelCreating(ModelBuilder modelBuilder)
  {
    modelBuilder.ApplyConfigurationsFromAssembly(typeof(EntityTypeConfigurationStockProduct).Assembly);
    base.OnModelCreating(modelBuilder);
  }
}
