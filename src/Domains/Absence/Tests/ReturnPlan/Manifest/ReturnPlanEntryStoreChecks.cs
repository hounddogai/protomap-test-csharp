namespace Acme.Workforce.Absence.Tests.ReturnPlan.Manifest;

using Acme.Workforce.Absence.ReturnPlan.Manifest;

/// <summary>Checks of manifest code, run by the project's runner.</summary>
public static class ReturnPlanEntryStoreChecks
{
    public static void MemoryReturnPlanEntryStoreFindsWhatItSaved()
    {
        var store = ReturnPlanEntryStore.NewMemoryReturnPlanEntryStore();
        var item = ReturnPlanEntry.NewReturnPlanEntry("code-160", "title-60");
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
        MemoryReturnPlanEntryStoreFindsWhatItSaved();
    }
}
