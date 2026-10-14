namespace Acme.Workforce.Engagement.Tests.ActionPlan.PlanSlot;

using Acme.Workforce.Engagement.ActionPlan.PlanSlot;

/// <summary>Checks of plan slot code, run by the project's runner.</summary>
public static class ActionPlanBundleStoreChecks
{
    public static void MemoryActionPlanBundleStoreFindsWhatItSaved()
    {
        var store = ActionPlanBundleStore.NewMemoryActionPlanBundleStore();
        var item = ActionPlanBundle.NewActionPlanBundle("code-513", "name-471");
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
        MemoryActionPlanBundleStoreFindsWhatItSaved();
    }
}
