namespace Acme.Workforce.Absence.Tests.ReturnPlan.Metric;

using Acme.Workforce.Absence.ReturnPlan.Metric;

/// <summary>Checks of metric code, run by the project's runner.</summary>
public static class AdaptiveReturnPlanMetricStoreChecks
{
    public static void MemoryAdaptiveReturnPlanMetricStoreFindsWhatItSaved()
    {
        var store = AdaptiveReturnPlanMetricStore.NewMemoryAdaptiveReturnPlanMetricStore();
        var item = AdaptiveReturnPlanMetric.NewAdaptiveReturnPlanMetric("code-589", "title-185");
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
        MemoryAdaptiveReturnPlanMetricStoreFindsWhatItSaved();
    }
}
