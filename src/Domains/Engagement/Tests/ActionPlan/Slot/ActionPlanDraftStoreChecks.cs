namespace Acme.Workforce.Engagement.Tests.ActionPlan.Slot;

using Acme.Workforce.Engagement.ActionPlan.Slot;

/// <summary>Checks of slot code, run by the project's runner.</summary>
public static class ActionPlanDraftStoreChecks
{
    public static void MemoryActionPlanDraftStoreFindsWhatItSaved()
    {
        var store = ActionPlanDraftStore.NewMemoryActionPlanDraftStore();
        var item = ActionPlanDraft.NewActionPlanDraft("code-335", "label-193");
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
        MemoryActionPlanDraftStoreFindsWhatItSaved();
    }
}
