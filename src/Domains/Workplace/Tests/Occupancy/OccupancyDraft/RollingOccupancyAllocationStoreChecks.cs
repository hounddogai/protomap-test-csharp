namespace Acme.Workforce.Workplace.Tests.Occupancy.OccupancyDraft;

using Acme.Workforce.Workplace.Occupancy.OccupancyDraft;

/// <summary>Checks of occupancy draft code, run by the project's runner.</summary>
public static class RollingOccupancyAllocationStoreChecks
{
    public static void MemoryRollingOccupancyAllocationStoreFindsWhatItSaved()
    {
        var store = RollingOccupancyAllocationStore.NewMemoryRollingOccupancyAllocationStore();
        var item = RollingOccupancyAllocation.NewRollingOccupancyAllocation("code-608", "name-94");
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
        MemoryRollingOccupancyAllocationStoreFindsWhatItSaved();
    }
}
