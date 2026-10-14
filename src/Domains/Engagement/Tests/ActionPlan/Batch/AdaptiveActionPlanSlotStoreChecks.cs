namespace Acme.Workforce.Engagement.Tests.ActionPlan.Batch;

using Acme.Workforce.Engagement.ActionPlan.Batch;

/// <summary>Checks of batch code, run by the project's runner.</summary>
public static class AdaptiveActionPlanSlotStoreChecks
{
    public static void MemoryAdaptiveActionPlanSlotStoreFindsWhatItSaved()
    {
        var store = AdaptiveActionPlanSlotStore.NewMemoryAdaptiveActionPlanSlotStore();
        var item = AdaptiveActionPlanSlot.NewAdaptiveActionPlanSlot("code-487", "title-215");
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
        MemoryAdaptiveActionPlanSlotStoreFindsWhatItSaved();
    }
}
