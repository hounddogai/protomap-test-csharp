namespace Acme.Workforce.Absence.Tests.Holiday.Variance;

using Acme.Workforce.Absence.Holiday.Variance;

/// <summary>Checks of variance code, run by the project's runner.</summary>
public static class RegionalHolidayAdjustmentStoreChecks
{
    public static void MemoryRegionalHolidayAdjustmentStoreFindsWhatItSaved()
    {
        var store = RegionalHolidayAdjustmentStore.NewMemoryRegionalHolidayAdjustmentStore();
        var item = RegionalHolidayAdjustment.NewRegionalHolidayAdjustment("code-458", "title-330");
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
        MemoryRegionalHolidayAdjustmentStoreFindsWhatItSaved();
    }
}
