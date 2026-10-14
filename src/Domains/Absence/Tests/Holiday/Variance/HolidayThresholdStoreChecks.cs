namespace Acme.Workforce.Absence.Tests.Holiday.Variance;

using Acme.Workforce.Absence.Holiday.Variance;

/// <summary>Checks of variance code, run by the project's runner.</summary>
public static class HolidayThresholdStoreChecks
{
    public static void MemoryHolidayThresholdStoreFindsWhatItSaved()
    {
        var store = HolidayThresholdStore.NewMemoryHolidayThresholdStore();
        var item = HolidayThreshold.NewHolidayThreshold("code-315", "title-572");
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
        MemoryHolidayThresholdStoreFindsWhatItSaved();
    }
}
