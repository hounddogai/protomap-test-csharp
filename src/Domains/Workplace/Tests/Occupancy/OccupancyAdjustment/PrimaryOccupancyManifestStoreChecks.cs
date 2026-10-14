namespace Acme.Workforce.Workplace.Tests.Occupancy.OccupancyAdjustment;

using Acme.Workforce.Workplace.Occupancy.OccupancyAdjustment;

/// <summary>Checks of occupancy adjustment code, run by the project's runner.</summary>
public static class PrimaryOccupancyManifestStoreChecks
{
    public static void MemoryPrimaryOccupancyManifestStoreFindsWhatItSaved()
    {
        var store = PrimaryOccupancyManifestStore.NewMemoryPrimaryOccupancyManifestStore();
        var item = PrimaryOccupancyManifest.NewPrimaryOccupancyManifest("code-196", "name-312");
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
        MemoryPrimaryOccupancyManifestStoreFindsWhatItSaved();
    }
}
