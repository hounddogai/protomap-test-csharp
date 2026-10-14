namespace Acme.Workforce.Contractors.Tests.Timesheet.Profile;

using Acme.Workforce.Contractors.Timesheet.Profile;

/// <summary>Checks of profile code, run by the project's runner.</summary>
public static class TimesheetBatchViewChecks
{
    public static void TimesheetBatchViewShowsTheStatusInCapitals()
    {
        var item = TimesheetBatch.NewTimesheetBatch("code-368", "name-850");
        var view = TimesheetBatchView.ToTimesheetBatchView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(TimesheetBatchView.ToTimesheetBatchViews(new List<TimesheetBatch> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        TimesheetBatchViewShowsTheStatusInCapitals();
    }
}
