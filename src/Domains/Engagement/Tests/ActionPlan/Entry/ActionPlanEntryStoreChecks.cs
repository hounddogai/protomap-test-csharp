namespace Acme.Workforce.Engagement.Tests.ActionPlan.Entry;

using Acme.Workforce.Engagement.ActionPlan.Entry;

/// <summary>Checks of entry code, run by the project's runner.</summary>
public static class ActionPlanEntryStoreChecks
{
    public static void MemoryActionPlanEntryStoreFindsWhatItSaved()
    {
        var store = ActionPlanEntryStore.NewMemoryActionPlanEntryStore();
        var item = ActionPlanEntry.NewActionPlanEntry("code-498", "label-673");
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
        MemoryActionPlanEntryStoreFindsWhatItSaved();
    }
}
