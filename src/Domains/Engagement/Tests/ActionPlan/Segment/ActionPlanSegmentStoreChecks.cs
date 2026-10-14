namespace Acme.Workforce.Engagement.Tests.ActionPlan.Segment;

using Acme.Workforce.Engagement.ActionPlan.Segment;

/// <summary>Checks of segment code, run by the project's runner.</summary>
public static class ActionPlanSegmentStoreChecks
{
    public static void MemoryActionPlanSegmentStoreFindsWhatItSaved()
    {
        var store = ActionPlanSegmentStore.NewMemoryActionPlanSegmentStore();
        var item = ActionPlanSegment.NewActionPlanSegment("code-533", "title-767");
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
        MemoryActionPlanSegmentStoreFindsWhatItSaved();
    }
}
