namespace Acme.Workforce.Compliance.Tests.Exception.Group;

using Acme.Workforce.Compliance.Exception.Group;

/// <summary>Checks of group code, run by the project's runner.</summary>
public static class ExceptionPlanStoreChecks
{
    public static void MemoryExceptionPlanStoreFindsWhatItSaved()
    {
        var store = ExceptionPlanStore.NewMemoryExceptionPlanStore();
        var item = ExceptionPlan.NewExceptionPlan("code-140", "label-622");
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
        MemoryExceptionPlanStoreFindsWhatItSaved();
    }
}
