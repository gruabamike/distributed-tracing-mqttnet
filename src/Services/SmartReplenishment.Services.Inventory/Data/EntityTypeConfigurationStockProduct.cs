using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartReplenishment.Shared.Model;

namespace SmartReplenishment.Services.Inventory.Data;

public class EntityTypeConfigurationStockProduct : EntityTypeConfigurationBase<StockArticle>
{
  protected override string TableName => "stock_products";

  protected override void ConfigureEntity(EntityTypeBuilder<StockArticle> builder)
  {
    builder.Property(sp => sp.Name)
      .HasColumnName("name")
      .HasMaxLength(MaxStringLength)
      .IsRequired();

    builder.HasIndex(sp => sp.Name)
      .IsUnique();

    builder.Property(sp => sp.Amount)
      .HasColumnName("amount")
      .IsRequired();

    builder.HasOne(sp => sp.StockConfiguration)
      .WithOne(sc => sc.StockArticle)
      .HasForeignKey<StockArticleConfiguration>(sp => sp.StockArticleId)
      .IsRequired()
      .OnDelete(DeleteBehavior.Cascade);
  }
}
