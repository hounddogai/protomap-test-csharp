namespace Acme.Workforce.Compliance.Tests.Exception.Plan;

using Acme.Workforce.Compliance.Exception.Plan;

/// <summary>Checks of plan code, run by the project's runner.</summary>
public static class PendingExceptionPlanStoreChecks
{
    public static void MemoryPendingExceptionPlanStoreFindsWhatItSaved()
    {
        var store = PendingExceptionPlanStore.NewMemoryPendingExceptionPlanStore();
        var item = PendingExceptionPlan.NewPendingExceptionPlan("code-812", "title-271");
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
        MemoryPendingExceptionPlanStoreFindsWhatItSaved();
    }
}
