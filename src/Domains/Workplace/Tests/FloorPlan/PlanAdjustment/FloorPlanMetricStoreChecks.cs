namespace Acme.Workforce.Workplace.Tests.FloorPlan.PlanAdjustment;

using Acme.Workforce.Workplace.FloorPlan.PlanAdjustment;

/// <summary>Checks of plan adjustment code, run by the project's runner.</summary>
public static class FloorPlanMetricStoreChecks
{
    public static void MemoryFloorPlanMetricStoreFindsWhatItSaved()
    {
        var store = FloorPlanMetricStore.NewMemoryFloorPlanMetricStore();
        var item = FloorPlanMetric.NewFloorPlanMetric("code-596", "name-36");
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
        MemoryFloorPlanMetricStoreFindsWhatItSaved();
    }
}
