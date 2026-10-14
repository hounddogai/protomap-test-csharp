namespace Acme.Workforce.Workplace.Tests.Occupancy.OccupancyPlan;

using Acme.Workforce.Workplace.Occupancy.OccupancyPlan;

/// <summary>Checks of occupancy plan code, run by the project's runner.</summary>
public static class OccupancyExceptionStoreChecks
{
    public static void MemoryOccupancyExceptionStoreFindsWhatItSaved()
    {
        var store = OccupancyExceptionStore.NewMemoryOccupancyExceptionStore();
        var item = OccupancyException.NewOccupancyException("code-207", "label-852");
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
        MemoryOccupancyExceptionStoreFindsWhatItSaved();
    }
}
