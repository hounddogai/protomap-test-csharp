namespace Acme.Workforce.Engagement.Tests.ActionPlan.Rollup;

using Acme.Workforce.Engagement.ActionPlan.Rollup;

/// <summary>Checks of rollup code, run by the project's runner.</summary>
public static class ActionPlanExceptionStoreChecks
{
    public static void MemoryActionPlanExceptionStoreFindsWhatItSaved()
    {
        var store = ActionPlanExceptionStore.NewMemoryActionPlanExceptionStore();
        var item = ActionPlanException.NewActionPlanException("code-905", "title-867");
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
        MemoryActionPlanExceptionStoreFindsWhatItSaved();
    }
}
