namespace Acme.Workforce.Contractors.Tests.Timesheet.Summary;

using Acme.Workforce.Contractors.Timesheet.Summary;

/// <summary>Checks of summary code, run by the project's runner.</summary>
public static class TimesheetSummaryStoreChecks
{
    public static void MemoryTimesheetSummaryStoreFindsWhatItSaved()
    {
        var store = TimesheetSummaryStore.NewMemoryTimesheetSummaryStore();
        var item = TimesheetSummary.NewTimesheetSummary("code-541", "name-859");
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
        MemoryTimesheetSummaryStoreFindsWhatItSaved();
    }
}
