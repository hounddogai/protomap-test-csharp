namespace Acme.Workforce.Engagement.Tests.ActionPlan.Metric;

using Acme.Workforce.Engagement.ActionPlan.Metric;

/// <summary>Checks of metric code, run by the project's runner.</summary>
public static class DefaultActionPlanDraftStoreChecks
{
    public static void MemoryDefaultActionPlanDraftStoreFindsWhatItSaved()
    {
        var store = DefaultActionPlanDraftStore.NewMemoryDefaultActionPlanDraftStore();
        var item = DefaultActionPlanDraft.NewDefaultActionPlanDraft("code-767", "label-759");
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
        MemoryDefaultActionPlanDraftStoreFindsWhatItSaved();
    }
}
