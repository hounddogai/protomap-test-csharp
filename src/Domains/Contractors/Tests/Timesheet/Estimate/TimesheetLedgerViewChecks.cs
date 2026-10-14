namespace Acme.Workforce.Contractors.Tests.Timesheet.Estimate;

using Acme.Workforce.Contractors.Timesheet.Estimate;

/// <summary>Checks of estimate code, run by the project's runner.</summary>
public static class TimesheetLedgerViewChecks
{
    public static void TimesheetLedgerViewShowsTheStatusInCapitals()
    {
        var item = TimesheetLedger.NewTimesheetLedger("code-949", "label-107");
        var view = TimesheetLedgerView.ToTimesheetLedgerView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(TimesheetLedgerView.ToTimesheetLedgerViews(new List<TimesheetLedger> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        TimesheetLedgerViewShowsTheStatusInCapitals();
    }
}
