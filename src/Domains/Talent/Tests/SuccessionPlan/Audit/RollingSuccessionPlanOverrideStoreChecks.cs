namespace Acme.Workforce.Talent.Tests.SuccessionPlan.Audit;

using Acme.Workforce.Talent.SuccessionPlan.Audit;

/// <summary>Checks of audit code, run by the project's runner.</summary>
public static class RollingSuccessionPlanOverrideStoreChecks
{
    public static void MemoryRollingSuccessionPlanOverrideStoreFindsWhatItSaved()
    {
        var store = RollingSuccessionPlanOverrideStore.NewMemoryRollingSuccessionPlanOverrideStore();
        var item = RollingSuccessionPlanOverride.NewRollingSuccessionPlanOverride("code-822", "name-325");
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
        MemoryRollingSuccessionPlanOverrideStoreFindsWhatItSaved();
    }
}
