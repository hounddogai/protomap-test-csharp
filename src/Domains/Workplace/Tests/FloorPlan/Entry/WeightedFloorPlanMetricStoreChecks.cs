namespace Acme.Workforce.Workplace.Tests.FloorPlan.Entry;

using Acme.Workforce.Workplace.FloorPlan.Entry;

/// <summary>Checks of entry code, run by the project's runner.</summary>
public static class WeightedFloorPlanMetricStoreChecks
{
    public static void MemoryWeightedFloorPlanMetricStoreFindsWhatItSaved()
    {
        var store = WeightedFloorPlanMetricStore.NewMemoryWeightedFloorPlanMetricStore();
        var item = WeightedFloorPlanMetric.NewWeightedFloorPlanMetric("code-885", "label-710");
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
        MemoryWeightedFloorPlanMetricStoreFindsWhatItSaved();
    }
}
