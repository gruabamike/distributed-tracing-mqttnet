﻿namespace SmartReplenishment.Clients.StockLevelEmitter.Settings;

internal class StockLevelEmitterSettings : IStockLevelEmitterSettings
{
  public required string ArticleName { get; set; }

  public required int StockDecreaseAmount { get; set; }
}
