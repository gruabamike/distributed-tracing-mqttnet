namespace SmartReplenishment.Shared.Model;

public class StockProduct : ModelBase
{
  public required string Name { get; set; }
  public required int Amount { get; set; }
  public StockConfiguration? StockConfiguration { get; set; }
}
