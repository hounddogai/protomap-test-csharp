namespace Acme.Workforce.Engagement.Tests.ActionPlan.Metric;

using Acme.Workforce.Engagement.ActionPlan.Metric;

/// <summary>Checks of metric code, run by the project's runner.</summary>
public static class CappedActionPlanDraftStoreChecks
{
    public static void MemoryCappedActionPlanDraftStoreFindsWhatItSaved()
    {
        var store = CappedActionPlanDraftStore.NewMemoryCappedActionPlanDraftStore();
        var item = CappedActionPlanDraft.NewCappedActionPlanDraft("code-950", "name-918");
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
        MemoryCappedActionPlanDraftStoreFindsWhatItSaved();
    }
}
