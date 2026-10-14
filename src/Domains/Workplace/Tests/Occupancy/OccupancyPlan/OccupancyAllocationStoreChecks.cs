namespace Acme.Workforce.Workplace.Tests.Occupancy.OccupancyPlan;

using Acme.Workforce.Workplace.Occupancy.OccupancyPlan;

/// <summary>Checks of occupancy plan code, run by the project's runner.</summary>
public static class OccupancyAllocationStoreChecks
{
    public static void MemoryOccupancyAllocationStoreFindsWhatItSaved()
    {
        var store = OccupancyAllocationStore.NewMemoryOccupancyAllocationStore();
        var item = OccupancyAllocation.NewOccupancyAllocation("code-721", "label-409");
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
        MemoryOccupancyAllocationStoreFindsWhatItSaved();
    }
}
