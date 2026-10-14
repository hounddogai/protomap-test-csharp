namespace Acme.Workforce.Contractors.Tests.Timesheet.Rollup;

using Acme.Workforce.Contractors.Timesheet.Rollup;

/// <summary>Checks of rollup code, run by the project's runner.</summary>
public static class TimesheetTargetViewChecks
{
    public static void TimesheetTargetViewShowsTheStatusInCapitals()
    {
        var item = TimesheetTarget.NewTimesheetTarget("code-951", "label-864");
        var view = TimesheetTargetView.ToTimesheetTargetView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(TimesheetTargetView.ToTimesheetTargetViews(new List<TimesheetTarget> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        TimesheetTargetViewShowsTheStatusInCapitals();
    }
}
