namespace Acme.Workforce.Engagement.Tests.ActionPlan.PlanSegment;

using Acme.Workforce.Engagement.ActionPlan.PlanSegment;

/// <summary>Checks of plan segment code, run by the project's runner.</summary>
public static class FallbackActionPlanSegmentStoreChecks
{
    public static void MemoryFallbackActionPlanSegmentStoreFindsWhatItSaved()
    {
        var store = FallbackActionPlanSegmentStore.NewMemoryFallbackActionPlanSegmentStore();
        var item = FallbackActionPlanSegment.NewFallbackActionPlanSegment("code-688", "name-672");
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
        MemoryFallbackActionPlanSegmentStoreFindsWhatItSaved();
    }
}
