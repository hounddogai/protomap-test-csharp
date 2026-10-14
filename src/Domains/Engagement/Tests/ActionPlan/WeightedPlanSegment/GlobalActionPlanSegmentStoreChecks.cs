namespace Acme.Workforce.Engagement.Tests.ActionPlan.WeightedPlanSegment;

using Acme.Workforce.Engagement.ActionPlan.WeightedPlanSegment;

/// <summary>Checks of weighted plan segment code, run by the project's runner.</summary>
public static class GlobalActionPlanSegmentStoreChecks
{
    public static void MemoryGlobalActionPlanSegmentStoreFindsWhatItSaved()
    {
        var store = GlobalActionPlanSegmentStore.NewMemoryGlobalActionPlanSegmentStore();
        var item = GlobalActionPlanSegment.NewGlobalActionPlanSegment("code-818", "name-561");
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
        MemoryGlobalActionPlanSegmentStoreFindsWhatItSaved();
    }
}
