namespace Acme.Workforce.Contractors.Tests.Timesheet.Reservation;

using Acme.Workforce.Contractors.Timesheet.Reservation;

/// <summary>Checks of reservation code, run by the project's runner.</summary>
public static class TimesheetScoreStoreChecks
{
    public static void MemoryTimesheetScoreStoreFindsWhatItSaved()
    {
        var store = TimesheetScoreStore.NewMemoryTimesheetScoreStore();
        var item = TimesheetScore.NewTimesheetScore("code-286", "title-173");
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
        MemoryTimesheetScoreStoreFindsWhatItSaved();
    }
}
