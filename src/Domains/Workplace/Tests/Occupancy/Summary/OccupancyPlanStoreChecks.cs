namespace Acme.Workforce.Workplace.Tests.Occupancy.Summary;

using Acme.Workforce.Workplace.Occupancy.Summary;

/// <summary>Checks of summary code, run by the project's runner.</summary>
public static class OccupancyPlanStoreChecks
{
    public static void MemoryOccupancyPlanStoreFindsWhatItSaved()
    {
        var store = OccupancyPlanStore.NewMemoryOccupancyPlanStore();
        var item = OccupancyPlan.NewOccupancyPlan("code-619", "name-855");
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
        MemoryOccupancyPlanStoreFindsWhatItSaved();
    }
}
