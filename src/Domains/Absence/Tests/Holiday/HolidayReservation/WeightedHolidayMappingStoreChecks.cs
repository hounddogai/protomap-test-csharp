namespace Acme.Workforce.Absence.Tests.Holiday.HolidayReservation;

using Acme.Workforce.Absence.Holiday.HolidayReservation;

/// <summary>Checks of holiday reservation code, run by the project's runner.</summary>
public static class WeightedHolidayMappingStoreChecks
{
    public static void MemoryWeightedHolidayMappingStoreFindsWhatItSaved()
    {
        var store = WeightedHolidayMappingStore.NewMemoryWeightedHolidayMappingStore();
        var item = WeightedHolidayMapping.NewWeightedHolidayMapping("code-565", "name-199");
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
        MemoryWeightedHolidayMappingStoreFindsWhatItSaved();
    }
}
