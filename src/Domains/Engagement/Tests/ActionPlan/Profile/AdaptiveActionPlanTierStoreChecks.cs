namespace Acme.Workforce.Engagement.Tests.ActionPlan.Profile;

using Acme.Workforce.Engagement.ActionPlan.Profile;

/// <summary>Checks of profile code, run by the project's runner.</summary>
public static class AdaptiveActionPlanTierStoreChecks
{
    public static void MemoryAdaptiveActionPlanTierStoreFindsWhatItSaved()
    {
        var store = AdaptiveActionPlanTierStore.NewMemoryAdaptiveActionPlanTierStore();
        var item = AdaptiveActionPlanTier.NewAdaptiveActionPlanTier("code-216", "name-832");
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
        MemoryAdaptiveActionPlanTierStoreFindsWhatItSaved();
    }
}
