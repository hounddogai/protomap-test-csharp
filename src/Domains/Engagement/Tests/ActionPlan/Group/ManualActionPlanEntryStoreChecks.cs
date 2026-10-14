namespace Acme.Workforce.Engagement.Tests.ActionPlan.Group;

using Acme.Workforce.Engagement.ActionPlan.Group;

/// <summary>Checks of group code, run by the project's runner.</summary>
public static class ManualActionPlanEntryStoreChecks
{
    public static void MemoryManualActionPlanEntryStoreFindsWhatItSaved()
    {
        var store = ManualActionPlanEntryStore.NewMemoryManualActionPlanEntryStore();
        var item = ManualActionPlanEntry.NewManualActionPlanEntry("code-354", "label-100");
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
        MemoryManualActionPlanEntryStoreFindsWhatItSaved();
    }
}
