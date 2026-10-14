namespace Acme.Workforce.Engagement.Tests.ActionPlan.Segment;

using Acme.Workforce.Engagement.ActionPlan.Segment;

/// <summary>Checks of segment code, run by the project's runner.</summary>
public static class ActionPlanMilestoneStoreChecks
{
    public static void MemoryActionPlanMilestoneStoreFindsWhatItSaved()
    {
        var store = ActionPlanMilestoneStore.NewMemoryActionPlanMilestoneStore();
        var item = ActionPlanMilestone.NewActionPlanMilestone("code-186", "label-774");
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
        MemoryActionPlanMilestoneStoreFindsWhatItSaved();
    }
}
