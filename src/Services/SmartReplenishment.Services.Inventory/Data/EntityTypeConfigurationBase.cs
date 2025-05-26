using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartReplenishment.Shared.Model;

namespace SmartReplenishment.Services.Inventory.Data;

public abstract class EntityTypeConfigurationBase<TModel> : IEntityTypeConfiguration<TModel> where TModel : ModelBase
{
  protected const int MaxStringLength = 256;

  protected abstract string TableName { get; }

  protected abstract void ConfigureEntity(EntityTypeBuilder<TModel> builder);

  public void Configure(EntityTypeBuilder<TModel> builder)
  {
    builder.ToTable(TableName);

    builder.HasKey(model => model.Id);

    builder.Property(model => model.Id)
      .HasColumnName("id")
      .ValueGeneratedOnAdd();

    ConfigureEntity(builder);
  }
}
