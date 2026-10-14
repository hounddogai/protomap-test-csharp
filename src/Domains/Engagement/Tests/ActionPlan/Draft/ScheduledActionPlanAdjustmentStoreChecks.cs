namespace Acme.Workforce.Engagement.Tests.ActionPlan.Draft;

using Acme.Workforce.Engagement.ActionPlan.Draft;

/// <summary>Checks of draft code, run by the project's runner.</summary>
public static class ScheduledActionPlanAdjustmentStoreChecks
{
    public static void MemoryScheduledActionPlanAdjustmentStoreFindsWhatItSaved()
    {
        var store = ScheduledActionPlanAdjustmentStore.NewMemoryScheduledActionPlanAdjustmentStore();
        var item = ScheduledActionPlanAdjustment.NewScheduledActionPlanAdjustment("code-485", "name-746");
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
        MemoryScheduledActionPlanAdjustmentStoreFindsWhatItSaved();
    }
}
