namespace Acme.Workforce.Absence.Tests.Holiday.Proposal;

using Acme.Workforce.Absence.Holiday.Proposal;

/// <summary>Checks of proposal code, run by the project's runner.</summary>
public static class HolidayAdjustmentStoreChecks
{
    public static void MemoryHolidayAdjustmentStoreFindsWhatItSaved()
    {
        var store = HolidayAdjustmentStore.NewMemoryHolidayAdjustmentStore();
        var item = HolidayAdjustment.NewHolidayAdjustment("code-164", "label-458");
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
        MemoryHolidayAdjustmentStoreFindsWhatItSaved();
    }
}
