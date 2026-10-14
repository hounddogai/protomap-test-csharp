namespace Acme.Workforce.Contractors.Tests.Timesheet.Window;

using Acme.Workforce.Contractors.Timesheet.Window;

/// <summary>Checks of window code, run by the project's runner.</summary>
public static class TimesheetVarianceViewChecks
{
    public static void TimesheetVarianceViewShowsTheStatusInCapitals()
    {
        var item = TimesheetVariance.NewTimesheetVariance("code-7", "label-670");
        var view = TimesheetVarianceView.ToTimesheetVarianceView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(TimesheetVarianceView.ToTimesheetVarianceViews(new List<TimesheetVariance> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        TimesheetVarianceViewShowsTheStatusInCapitals();
    }
}
