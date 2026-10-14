namespace Acme.Workforce.Workplace.Tests.Occupancy.Estimate;

using Acme.Workforce.Workplace.Occupancy.Estimate;

/// <summary>Checks of estimate code, run by the project's runner.</summary>
public static class RegionalOccupancyTierStoreChecks
{
    public static void MemoryRegionalOccupancyTierStoreFindsWhatItSaved()
    {
        var store = RegionalOccupancyTierStore.NewMemoryRegionalOccupancyTierStore();
        var item = RegionalOccupancyTier.NewRegionalOccupancyTier("code-668", "name-299");
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
        MemoryRegionalOccupancyTierStoreFindsWhatItSaved();
    }
}
