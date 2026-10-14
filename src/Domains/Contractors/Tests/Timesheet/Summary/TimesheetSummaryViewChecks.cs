namespace Acme.Workforce.Contractors.Tests.Timesheet.Summary;

using Acme.Workforce.Contractors.Timesheet.Summary;

/// <summary>Checks of summary code, run by the project's runner.</summary>
public static class TimesheetSummaryViewChecks
{
    public static void TimesheetSummaryViewShowsTheStatusInCapitals()
    {
        var item = TimesheetSummary.NewTimesheetSummary("code-541", "name-859");
        var view = TimesheetSummaryView.ToTimesheetSummaryView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(TimesheetSummaryView.ToTimesheetSummaryViews(new List<TimesheetSummary> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        TimesheetSummaryViewShowsTheStatusInCapitals();
    }
}
