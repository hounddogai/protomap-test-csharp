namespace Acme.Workforce.Talent.Tests.SuccessionPlan.PlanVariance;

using Acme.Workforce.Talent.SuccessionPlan.PlanVariance;

/// <summary>Checks of plan variance code, run by the project's runner.</summary>
public static class DefaultSuccessionPlanVarianceStoreChecks
{
    public static void MemoryDefaultSuccessionPlanVarianceStoreFindsWhatItSaved()
    {
        var store = DefaultSuccessionPlanVarianceStore.NewMemoryDefaultSuccessionPlanVarianceStore();
        var item = DefaultSuccessionPlanVariance.NewDefaultSuccessionPlanVariance("code-263", "title-319");
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
        MemoryDefaultSuccessionPlanVarianceStoreFindsWhatItSaved();
    }
}
