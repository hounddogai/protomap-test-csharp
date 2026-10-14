namespace Acme.Workforce.Absence.Tests.ReturnPlan.Metric;

using Acme.Workforce.Absence.ReturnPlan.Metric;

/// <summary>Checks of metric code, run by the project's runner.</summary>
public static class StrictReturnPlanMetricStoreChecks
{
    public static void MemoryStrictReturnPlanMetricStoreFindsWhatItSaved()
    {
        var store = StrictReturnPlanMetricStore.NewMemoryStrictReturnPlanMetricStore();
        var item = StrictReturnPlanMetric.NewStrictReturnPlanMetric("code-948", "label-509");
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
        MemoryStrictReturnPlanMetricStoreFindsWhatItSaved();
    }
}
