namespace Acme.Workforce.Talent.Tests.SuccessionPlan.Rollup;

using Acme.Workforce.Talent.SuccessionPlan.Rollup;

/// <summary>Checks of rollup code, run by the project's runner.</summary>
public static class SuccessionPlanRollupStoreChecks
{
    public static void MemorySuccessionPlanRollupStoreFindsWhatItSaved()
    {
        var store = SuccessionPlanRollupStore.NewMemorySuccessionPlanRollupStore();
        var item = SuccessionPlanRollup.NewSuccessionPlanRollup("code-961", "label-167");
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
        MemorySuccessionPlanRollupStoreFindsWhatItSaved();
    }
}
