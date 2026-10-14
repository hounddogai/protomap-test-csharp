namespace Acme.Workforce.Workplace.Tests.FloorPlan.Cycle;

using Acme.Workforce.Workplace.FloorPlan.Cycle;

/// <summary>Checks of cycle code, run by the project's runner.</summary>
public static class AdaptiveFloorPlanMetricStoreChecks
{
    public static void MemoryAdaptiveFloorPlanMetricStoreFindsWhatItSaved()
    {
        var store = AdaptiveFloorPlanMetricStore.NewMemoryAdaptiveFloorPlanMetricStore();
        var item = AdaptiveFloorPlanMetric.NewAdaptiveFloorPlanMetric("code-156", "label-15");
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
        MemoryAdaptiveFloorPlanMetricStoreFindsWhatItSaved();
    }
}
