namespace Acme.Workforce.Workplace.Tests.Occupancy.Checkpoint;

using Acme.Workforce.Workplace.Occupancy.Checkpoint;

/// <summary>Checks of checkpoint code, run by the project's runner.</summary>
public static class OccupancyMetricStoreChecks
{
    public static void MemoryOccupancyMetricStoreFindsWhatItSaved()
    {
        var store = OccupancyMetricStore.NewMemoryOccupancyMetricStore();
        var item = OccupancyMetric.NewOccupancyMetric("code-361", "name-481");
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
        MemoryOccupancyMetricStoreFindsWhatItSaved();
    }
}
