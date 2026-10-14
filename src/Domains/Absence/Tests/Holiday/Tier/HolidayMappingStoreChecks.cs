namespace Acme.Workforce.Absence.Tests.Holiday.Tier;

using Acme.Workforce.Absence.Holiday.Tier;

/// <summary>Checks of tier code, run by the project's runner.</summary>
public static class HolidayMappingStoreChecks
{
    public static void MemoryHolidayMappingStoreFindsWhatItSaved()
    {
        var store = HolidayMappingStore.NewMemoryHolidayMappingStore();
        var item = HolidayMapping.NewHolidayMapping("code-726", "label-103");
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
        MemoryHolidayMappingStoreFindsWhatItSaved();
    }
}
