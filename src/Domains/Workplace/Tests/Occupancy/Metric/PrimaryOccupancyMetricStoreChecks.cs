namespace Acme.Workforce.Workplace.Tests.Occupancy.Metric;

using Acme.Workforce.Workplace.Occupancy.Metric;

/// <summary>Checks of metric code, run by the project's runner.</summary>
public static class PrimaryOccupancyMetricStoreChecks
{
    public static void MemoryPrimaryOccupancyMetricStoreFindsWhatItSaved()
    {
        var store = PrimaryOccupancyMetricStore.NewMemoryPrimaryOccupancyMetricStore();
        var item = PrimaryOccupancyMetric.NewPrimaryOccupancyMetric("code-791", "label-803");
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
        MemoryPrimaryOccupancyMetricStoreFindsWhatItSaved();
    }
}
