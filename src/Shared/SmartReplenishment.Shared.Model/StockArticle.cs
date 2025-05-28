namespace SmartReplenishment.Shared.Model;

public class StockArticle : ModelBase
{
  public required string Name { get; set; }
  public required int Amount { get; set; }
  public StockArticleConfiguration? StockConfiguration { get; set; }
}
