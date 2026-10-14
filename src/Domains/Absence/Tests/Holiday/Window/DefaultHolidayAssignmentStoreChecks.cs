namespace Acme.Workforce.Absence.Tests.Holiday.Window;

using Acme.Workforce.Absence.Holiday.Window;

/// <summary>Checks of window code, run by the project's runner.</summary>
public static class DefaultHolidayAssignmentStoreChecks
{
    public static void MemoryDefaultHolidayAssignmentStoreFindsWhatItSaved()
    {
        var store = DefaultHolidayAssignmentStore.NewMemoryDefaultHolidayAssignmentStore();
        var item = DefaultHolidayAssignment.NewDefaultHolidayAssignment("code-155", "title-450");
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
        MemoryDefaultHolidayAssignmentStoreFindsWhatItSaved();
    }
}
