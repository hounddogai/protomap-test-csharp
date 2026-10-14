namespace Acme.Workforce.Engagement.Tests.ActionPlan.Claim;

using Acme.Workforce.Engagement.ActionPlan.Claim;

/// <summary>Checks of claim code, run by the project's runner.</summary>
public static class DefaultActionPlanRollupStoreChecks
{
    public static void MemoryDefaultActionPlanRollupStoreFindsWhatItSaved()
    {
        var store = DefaultActionPlanRollupStore.NewMemoryDefaultActionPlanRollupStore();
        var item = DefaultActionPlanRollup.NewDefaultActionPlanRollup("code-883", "label-183");
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
        MemoryDefaultActionPlanRollupStoreFindsWhatItSaved();
    }
}
