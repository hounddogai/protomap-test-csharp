namespace Acme.Workforce.Engagement.Tests.ActionPlan.WeightedPlanSegment;

using Acme.Workforce.Engagement.ActionPlan.WeightedPlanSegment;

/// <summary>Checks of weighted plan segment code, run by the project's runner.</summary>
public static class GlobalActionPlanEntryStoreChecks
{
    public static void MemoryGlobalActionPlanEntryStoreFindsWhatItSaved()
    {
        var store = GlobalActionPlanEntryStore.NewMemoryGlobalActionPlanEntryStore();
        var item = GlobalActionPlanEntry.NewGlobalActionPlanEntry("code-111", "title-153");
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
        MemoryGlobalActionPlanEntryStoreFindsWhatItSaved();
    }
}
