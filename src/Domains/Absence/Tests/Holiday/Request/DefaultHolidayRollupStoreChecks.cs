namespace Acme.Workforce.Absence.Tests.Holiday.Request;

using Acme.Workforce.Absence.Holiday.Request;

/// <summary>Checks of request code, run by the project's runner.</summary>
public static class DefaultHolidayRollupStoreChecks
{
    public static void MemoryDefaultHolidayRollupStoreFindsWhatItSaved()
    {
        var store = DefaultHolidayRollupStore.NewMemoryDefaultHolidayRollupStore();
        var item = DefaultHolidayRollup.NewDefaultHolidayRollup("code-725", "label-644");
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
        MemoryDefaultHolidayRollupStoreFindsWhatItSaved();
    }
}
