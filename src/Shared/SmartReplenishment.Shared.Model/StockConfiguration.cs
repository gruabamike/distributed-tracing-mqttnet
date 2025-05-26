namespace SmartReplenishment.Shared.Model;

public class StockConfiguration : ModelBase
{
  public Guid StockProductId { get; set; }
  public StockProduct? StockProduct { get; set; }
  public int MinStockThreshold { get; set; }
}
