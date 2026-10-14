namespace Acme.Workforce.Talent.Tests.SuccessionPlan.PlanRollup;

using Acme.Workforce.Talent.SuccessionPlan.PlanRollup;

/// <summary>Checks of plan rollup code, run by the project's runner.</summary>
public static class PrimarySuccessionPlanRollupStoreChecks
{
    public static void MemoryPrimarySuccessionPlanRollupStoreFindsWhatItSaved()
    {
        var store = PrimarySuccessionPlanRollupStore.NewMemoryPrimarySuccessionPlanRollupStore();
        var item = PrimarySuccessionPlanRollup.NewPrimarySuccessionPlanRollup("code-697", "label-208");
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
        MemoryPrimarySuccessionPlanRollupStoreFindsWhatItSaved();
    }
}
