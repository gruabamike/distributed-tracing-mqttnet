namespace SmartReplenishment.Shared.Model;

public class StockArticleConfiguration : ModelBase
{
  public Guid StockArticleId { get; set; }
  public StockArticle? StockArticle { get; set; }
  public int MinStockThreshold { get; set; }
}
