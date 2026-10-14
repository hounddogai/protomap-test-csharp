namespace Acme.Workforce.Contractors.Tests.Timesheet.OverrideItem;

using Acme.Workforce.Contractors.Timesheet.OverrideItem;

/// <summary>Checks of override item code, run by the project's runner.</summary>
public static class TimesheetOverrideViewChecks
{
    public static void TimesheetOverrideViewShowsTheStatusInCapitals()
    {
        var item = TimesheetOverride.NewTimesheetOverride("code-410", "label-946");
        var view = TimesheetOverrideView.ToTimesheetOverrideView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(TimesheetOverrideView.ToTimesheetOverrideViews(new List<TimesheetOverride> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        TimesheetOverrideViewShowsTheStatusInCapitals();
    }
}
