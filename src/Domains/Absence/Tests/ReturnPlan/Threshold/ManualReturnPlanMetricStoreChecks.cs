namespace Acme.Workforce.Absence.Tests.ReturnPlan.Threshold;

using Acme.Workforce.Absence.ReturnPlan.Threshold;

/// <summary>Checks of threshold code, run by the project's runner.</summary>
public static class ManualReturnPlanMetricStoreChecks
{
    public static void MemoryManualReturnPlanMetricStoreFindsWhatItSaved()
    {
        var store = ManualReturnPlanMetricStore.NewMemoryManualReturnPlanMetricStore();
        var item = ManualReturnPlanMetric.NewManualReturnPlanMetric("code-40", "name-521");
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
        MemoryManualReturnPlanMetricStoreFindsWhatItSaved();
    }
}
