namespace Acme.Workforce.Workplace.Tests.FloorPlan.Metric;

using Acme.Workforce.Workplace.FloorPlan.Metric;

/// <summary>Checks of metric code, run by the project's runner.</summary>
public static class FallbackFloorPlanMetricStoreChecks
{
    public static void MemoryFallbackFloorPlanMetricStoreFindsWhatItSaved()
    {
        var store = FallbackFloorPlanMetricStore.NewMemoryFallbackFloorPlanMetricStore();
        var item = FallbackFloorPlanMetric.NewFallbackFloorPlanMetric("code-11", "label-899");
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
        MemoryFallbackFloorPlanMetricStoreFindsWhatItSaved();
    }
}
