namespace Acme.Workforce.Workplace.Tests.Occupancy.Target;

using Acme.Workforce.Workplace.Occupancy.Target;

/// <summary>Checks of target code, run by the project's runner.</summary>
public static class StrictOccupancyAdjustmentStoreChecks
{
    public static void MemoryStrictOccupancyAdjustmentStoreFindsWhatItSaved()
    {
        var store = StrictOccupancyAdjustmentStore.NewMemoryStrictOccupancyAdjustmentStore();
        var item = StrictOccupancyAdjustment.NewStrictOccupancyAdjustment("code-213", "label-201");
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
        MemoryStrictOccupancyAdjustmentStoreFindsWhatItSaved();
    }
}
