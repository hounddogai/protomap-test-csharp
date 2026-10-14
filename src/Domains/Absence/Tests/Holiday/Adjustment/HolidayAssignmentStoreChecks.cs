namespace Acme.Workforce.Absence.Tests.Holiday.Adjustment;

using Acme.Workforce.Absence.Holiday.Adjustment;

/// <summary>Checks of adjustment code, run by the project's runner.</summary>
public static class HolidayAssignmentStoreChecks
{
    public static void MemoryHolidayAssignmentStoreFindsWhatItSaved()
    {
        var store = HolidayAssignmentStore.NewMemoryHolidayAssignmentStore();
        var item = HolidayAssignment.NewHolidayAssignment("code-105", "name-749");
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
        MemoryHolidayAssignmentStoreFindsWhatItSaved();
    }
}
