namespace Acme.Workforce.Absence.Tests.ReturnPlan.Summary;

using Acme.Workforce.Absence.ReturnPlan.Summary;

/// <summary>Checks of summary code, run by the project's runner.</summary>
public static class LenientReturnPlanOverrideStoreChecks
{
    public static void MemoryLenientReturnPlanOverrideStoreFindsWhatItSaved()
    {
        var store = LenientReturnPlanOverrideStore.NewMemoryLenientReturnPlanOverrideStore();
        var item = LenientReturnPlanOverride.NewLenientReturnPlanOverride("code-554", "name-821");
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
        MemoryLenientReturnPlanOverrideStoreFindsWhatItSaved();
    }
}
