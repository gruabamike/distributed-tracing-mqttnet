using System.Diagnostics.Metrics;

namespace SmartReplenishment.Services.InventoryService.Metrics;

public class InventoryMetrics
{
    private static readonly Meter meter = new(nameof(InventoryService));
    private static readonly Counter<int> movementCounter = meter.CreateCounter<int>("inventory_movements");

    public void TrackMovement()
    {
        movementCounter.Add(1);
    }
}
