namespace SmartReplenishment.Clients.StockLevelEmitter.Settings;

public interface IStockLevelEmitterSettings
{
  public const string StockLevelEmitterSettingsKey = "StockLevelEmitterSettings";

  string ProductName { get; set; }

  int StockDecreaseAmount { get; set; }
}
