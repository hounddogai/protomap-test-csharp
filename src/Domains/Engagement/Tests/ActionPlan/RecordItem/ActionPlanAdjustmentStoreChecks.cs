namespace Acme.Workforce.Engagement.Tests.ActionPlan.RecordItem;

using Acme.Workforce.Engagement.ActionPlan.RecordItem;

/// <summary>Checks of record item code, run by the project's runner.</summary>
public static class ActionPlanAdjustmentStoreChecks
{
    public static void MemoryActionPlanAdjustmentStoreFindsWhatItSaved()
    {
        var store = ActionPlanAdjustmentStore.NewMemoryActionPlanAdjustmentStore();
        var item = ActionPlanAdjustment.NewActionPlanAdjustment("code-766", "title-665");
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
        MemoryActionPlanAdjustmentStoreFindsWhatItSaved();
    }
}
