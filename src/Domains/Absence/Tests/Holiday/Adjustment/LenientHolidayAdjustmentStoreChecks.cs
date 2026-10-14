namespace Acme.Workforce.Absence.Tests.Holiday.Adjustment;

using Acme.Workforce.Absence.Holiday.Adjustment;

/// <summary>Checks of adjustment code, run by the project's runner.</summary>
public static class LenientHolidayAdjustmentStoreChecks
{
    public static void MemoryLenientHolidayAdjustmentStoreFindsWhatItSaved()
    {
        var store = LenientHolidayAdjustmentStore.NewMemoryLenientHolidayAdjustmentStore();
        var item = LenientHolidayAdjustment.NewLenientHolidayAdjustment("code-210", "name-107");
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
        MemoryLenientHolidayAdjustmentStoreFindsWhatItSaved();
    }
}
