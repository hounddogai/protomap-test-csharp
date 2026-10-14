namespace Acme.Workforce.Absence.Tests.Holiday.Reservation;

using Acme.Workforce.Absence.Holiday.Reservation;

/// <summary>Checks of reservation code, run by the project's runner.</summary>
public static class StrictHolidayAllocationStoreChecks
{
    public static void MemoryStrictHolidayAllocationStoreFindsWhatItSaved()
    {
        var store = StrictHolidayAllocationStore.NewMemoryStrictHolidayAllocationStore();
        var item = StrictHolidayAllocation.NewStrictHolidayAllocation("code-897", "name-527");
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
        MemoryStrictHolidayAllocationStoreFindsWhatItSaved();
    }
}
