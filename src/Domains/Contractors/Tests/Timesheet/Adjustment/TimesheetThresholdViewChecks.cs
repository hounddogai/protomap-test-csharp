namespace Acme.Workforce.Contractors.Tests.Timesheet.Adjustment;

using Acme.Workforce.Contractors.Timesheet.Adjustment;

/// <summary>Checks of adjustment code, run by the project's runner.</summary>
public static class TimesheetThresholdViewChecks
{
    public static void TimesheetThresholdViewShowsTheStatusInCapitals()
    {
        var item = TimesheetThreshold.NewTimesheetThreshold("code-20", "title-196");
        var view = TimesheetThresholdView.ToTimesheetThresholdView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(TimesheetThresholdView.ToTimesheetThresholdViews(new List<TimesheetThreshold> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        TimesheetThresholdViewShowsTheStatusInCapitals();
    }
}
