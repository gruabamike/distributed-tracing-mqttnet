namespace SmartReplenishment.Clients.StockLevelEmitter.Settings;

public interface IStockLevelEmitterSettings
{
  public const string StockLevelEmitterSettingsKey = "StockLevelEmitterSettings";

  string ArticleName { get; set; }

  int StockDecreaseAmount { get; set; }
}
