namespace Acme.Workforce.Workplace.Tests.FloorPlan.Metric;

using Acme.Workforce.Workplace.FloorPlan.Metric;

/// <summary>Checks of metric code, run by the project's runner.</summary>
public static class ManualFloorPlanMetricStoreChecks
{
    public static void MemoryManualFloorPlanMetricStoreFindsWhatItSaved()
    {
        var store = ManualFloorPlanMetricStore.NewMemoryManualFloorPlanMetricStore();
        var item = ManualFloorPlanMetric.NewManualFloorPlanMetric("code-45", "label-706");
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
        MemoryManualFloorPlanMetricStoreFindsWhatItSaved();
    }
}
