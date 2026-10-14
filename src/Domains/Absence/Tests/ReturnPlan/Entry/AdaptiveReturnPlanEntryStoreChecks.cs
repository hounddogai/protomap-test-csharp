namespace Acme.Workforce.Absence.Tests.ReturnPlan.Entry;

using Acme.Workforce.Absence.ReturnPlan.Entry;

/// <summary>Checks of entry code, run by the project's runner.</summary>
public static class AdaptiveReturnPlanEntryStoreChecks
{
    public static void MemoryAdaptiveReturnPlanEntryStoreFindsWhatItSaved()
    {
        var store = AdaptiveReturnPlanEntryStore.NewMemoryAdaptiveReturnPlanEntryStore();
        var item = AdaptiveReturnPlanEntry.NewAdaptiveReturnPlanEntry("code-535", "name-366");
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
        MemoryAdaptiveReturnPlanEntryStoreFindsWhatItSaved();
    }
}
