namespace Acme.Workforce.Contractors.Tests.Timesheet.Reservation;

using Acme.Workforce.Contractors.Timesheet.Reservation;

/// <summary>Checks of reservation code, run by the project's runner.</summary>
public static class TimesheetReservationStoreChecks
{
    public static void MemoryTimesheetReservationStoreFindsWhatItSaved()
    {
        var store = TimesheetReservationStore.NewMemoryTimesheetReservationStore();
        var item = TimesheetReservation.NewTimesheetReservation("code-891", "label-407");
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
        MemoryTimesheetReservationStoreFindsWhatItSaved();
    }
}
