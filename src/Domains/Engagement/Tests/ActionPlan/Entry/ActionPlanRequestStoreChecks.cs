namespace Acme.Workforce.Engagement.Tests.ActionPlan.Entry;

using Acme.Workforce.Engagement.ActionPlan.Entry;

/// <summary>Checks of entry code, run by the project's runner.</summary>
public static class ActionPlanRequestStoreChecks
{
    public static void MemoryActionPlanRequestStoreFindsWhatItSaved()
    {
        var store = ActionPlanRequestStore.NewMemoryActionPlanRequestStore();
        var item = ActionPlanRequest.NewActionPlanRequest("code-526", "name-993");
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
        MemoryActionPlanRequestStoreFindsWhatItSaved();
    }
}
