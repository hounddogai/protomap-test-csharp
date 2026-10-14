namespace Acme.Workforce.Absence.Tests.Holiday.Mapping;

using Acme.Workforce.Absence.Holiday.Mapping;

/// <summary>Checks of mapping code, run by the project's runner.</summary>
public static class HolidayRollupStoreChecks
{
    public static void MemoryHolidayRollupStoreFindsWhatItSaved()
    {
        var store = HolidayRollupStore.NewMemoryHolidayRollupStore();
        var item = HolidayRollup.NewHolidayRollup("code-869", "label-564");
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
        MemoryHolidayRollupStoreFindsWhatItSaved();
    }
}
