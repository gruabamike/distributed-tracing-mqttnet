using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartReplenishment.Shared.Model;

namespace SmartReplenishment.Services.Inventory.Data;

public class EntityTypeConfigurationStockConfiguration : EntityTypeConfigurationBase<StockConfiguration>
{
  protected override string TableName => "stock_configurations";

  protected override void ConfigureEntity(EntityTypeBuilder<StockConfiguration> builder)
  {
    builder.Property(sc => sc.StockProductId)
      .HasColumnName("stock_product_id")
      .IsRequired();

    builder.Property(sc => sc.MinStockThreshold)
      .HasColumnName("min_stock_threshold")
      .IsRequired();
  }
}
