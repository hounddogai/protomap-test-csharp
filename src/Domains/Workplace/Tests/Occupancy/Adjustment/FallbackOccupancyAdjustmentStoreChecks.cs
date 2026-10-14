namespace Acme.Workforce.Workplace.Tests.Occupancy.Adjustment;

using Acme.Workforce.Workplace.Occupancy.Adjustment;

/// <summary>Checks of adjustment code, run by the project's runner.</summary>
public static class FallbackOccupancyAdjustmentStoreChecks
{
    public static void MemoryFallbackOccupancyAdjustmentStoreFindsWhatItSaved()
    {
        var store = FallbackOccupancyAdjustmentStore.NewMemoryFallbackOccupancyAdjustmentStore();
        var item = FallbackOccupancyAdjustment.NewFallbackOccupancyAdjustment("code-862", "label-726");
        store.Save(item);
        if (store.Find(item.Code) is not { } found)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(found.Code, item.Code, "code");
        Check.Equal(store.List().Count, 1, "count");
    }

    public static void RunAll()
    {
        MemoryFallbackOccupancyAdjustmentStoreFindsWhatItSaved();
    }
}
