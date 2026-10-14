namespace Acme.Workforce.Absence.Tests.ReturnPlan.Template;

using Acme.Workforce.Absence.ReturnPlan.Template;

/// <summary>Checks of template code, run by the project's runner.</summary>
public static class ReturnPlanMetricStoreChecks
{
    public static void MemoryReturnPlanMetricStoreFindsWhatItSaved()
    {
        var store = ReturnPlanMetricStore.NewMemoryReturnPlanMetricStore();
        var item = ReturnPlanMetric.NewReturnPlanMetric("code-288", "label-988");
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
        MemoryReturnPlanMetricStoreFindsWhatItSaved();
    }
}
