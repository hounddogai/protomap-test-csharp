namespace Acme.Workforce.Absence.Tests.ReturnPlan.PlanAudit;

using Acme.Workforce.Absence.ReturnPlan.PlanAudit;

/// <summary>Checks of plan audit code, run by the project's runner.</summary>
public static class WeightedReturnPlanOverrideStoreChecks
{
    public static void MemoryWeightedReturnPlanOverrideStoreFindsWhatItSaved()
    {
        var store = WeightedReturnPlanOverrideStore.NewMemoryWeightedReturnPlanOverrideStore();
        var item = WeightedReturnPlanOverride.NewWeightedReturnPlanOverride("code-322", "label-710");
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
        MemoryWeightedReturnPlanOverrideStoreFindsWhatItSaved();
    }
}
