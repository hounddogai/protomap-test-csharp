namespace Acme.Workforce.Absence.Tests.ReturnPlan.Bundle;

using Acme.Workforce.Absence.ReturnPlan.Bundle;

/// <summary>Checks of bundle code, run by the project's runner.</summary>
public static class ReturnPlanBundleStoreChecks
{
    public static void MemoryReturnPlanBundleStoreFindsWhatItSaved()
    {
        var store = ReturnPlanBundleStore.NewMemoryReturnPlanBundleStore();
        var item = ReturnPlanBundle.NewReturnPlanBundle("code-189", "name-542");
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
        MemoryReturnPlanBundleStoreFindsWhatItSaved();
    }
}
