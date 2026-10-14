namespace Acme.Workforce.Engagement.Tests.ActionPlan.Mapping;

using Acme.Workforce.Engagement.ActionPlan.Mapping;

/// <summary>Checks of mapping code, run by the project's runner.</summary>
public static class ActionPlanSummaryStoreChecks
{
    public static void MemoryActionPlanSummaryStoreFindsWhatItSaved()
    {
        var store = ActionPlanSummaryStore.NewMemoryActionPlanSummaryStore();
        var item = ActionPlanSummary.NewActionPlanSummary("code-506", "name-294");
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
        MemoryActionPlanSummaryStoreFindsWhatItSaved();
    }
}
