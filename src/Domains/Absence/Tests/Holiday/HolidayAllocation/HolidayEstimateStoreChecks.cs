namespace Acme.Workforce.Absence.Tests.Holiday.HolidayAllocation;

using Acme.Workforce.Absence.Holiday.HolidayAllocation;

/// <summary>Checks of holiday allocation code, run by the project's runner.</summary>
public static class HolidayEstimateStoreChecks
{
    public static void MemoryHolidayEstimateStoreFindsWhatItSaved()
    {
        var store = HolidayEstimateStore.NewMemoryHolidayEstimateStore();
        var item = HolidayEstimate.NewHolidayEstimate("code-820", "title-100");
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
        MemoryHolidayEstimateStoreFindsWhatItSaved();
    }
}
