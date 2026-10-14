namespace Acme.Workforce.Talent.Tests.SuccessionPlan.Milestone;

using Acme.Workforce.Talent.SuccessionPlan.Milestone;

/// <summary>Checks of milestone code, run by the project's runner.</summary>
public static class SuccessionPlanOverrideStoreChecks
{
    public static void MemorySuccessionPlanOverrideStoreFindsWhatItSaved()
    {
        var store = SuccessionPlanOverrideStore.NewMemorySuccessionPlanOverrideStore();
        var item = SuccessionPlanOverride.NewSuccessionPlanOverride("code-926", "name-300");
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
        MemorySuccessionPlanOverrideStoreFindsWhatItSaved();
    }
}
