namespace Acme.Workforce.Engagement.Tests.ActionPlan.Bundle;

using Acme.Workforce.Engagement.ActionPlan.Bundle;

/// <summary>Checks of bundle code, run by the project's runner.</summary>
public static class ActionPlanItemStoreChecks
{
    public static void MemoryActionPlanItemStoreFindsWhatItSaved()
    {
        var store = ActionPlanItemStore.NewMemoryActionPlanItemStore();
        var item = ActionPlanItem.NewActionPlanItem("code-818", "name-425");
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
        MemoryActionPlanItemStoreFindsWhatItSaved();
    }
}
