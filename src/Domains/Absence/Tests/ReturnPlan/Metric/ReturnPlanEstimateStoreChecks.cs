namespace Acme.Workforce.Absence.Tests.ReturnPlan.Metric;

using Acme.Workforce.Absence.ReturnPlan.Metric;

/// <summary>Checks of metric code, run by the project's runner.</summary>
public static class ReturnPlanEstimateStoreChecks
{
    public static void MemoryReturnPlanEstimateStoreFindsWhatItSaved()
    {
        var store = ReturnPlanEstimateStore.NewMemoryReturnPlanEstimateStore();
        var item = ReturnPlanEstimate.NewReturnPlanEstimate("code-672", "name-674");
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
        MemoryReturnPlanEstimateStoreFindsWhatItSaved();
    }
}
