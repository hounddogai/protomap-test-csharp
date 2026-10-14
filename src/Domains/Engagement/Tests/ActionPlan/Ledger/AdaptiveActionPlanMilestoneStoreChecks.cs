namespace Acme.Workforce.Engagement.Tests.ActionPlan.Ledger;

using Acme.Workforce.Engagement.ActionPlan.Ledger;

/// <summary>Checks of ledger code, run by the project's runner.</summary>
public static class AdaptiveActionPlanMilestoneStoreChecks
{
    public static void MemoryAdaptiveActionPlanMilestoneStoreFindsWhatItSaved()
    {
        var store = AdaptiveActionPlanMilestoneStore.NewMemoryAdaptiveActionPlanMilestoneStore();
        var item = AdaptiveActionPlanMilestone.NewAdaptiveActionPlanMilestone("code-18", "title-133");
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
        MemoryAdaptiveActionPlanMilestoneStoreFindsWhatItSaved();
    }
}
