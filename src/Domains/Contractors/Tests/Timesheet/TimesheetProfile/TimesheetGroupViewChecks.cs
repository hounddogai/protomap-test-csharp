namespace Acme.Workforce.Contractors.Tests.Timesheet.TimesheetProfile;

using Acme.Workforce.Contractors.Timesheet.TimesheetProfile;

/// <summary>Checks of timesheet profile code, run by the project's runner.</summary>
public static class TimesheetGroupViewChecks
{
    public static void TimesheetGroupViewShowsTheStatusInCapitals()
    {
        var item = TimesheetGroup.NewTimesheetGroup("code-131", "title-654");
        var view = TimesheetGroupView.ToTimesheetGroupView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(TimesheetGroupView.ToTimesheetGroupViews(new List<TimesheetGroup> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        TimesheetGroupViewShowsTheStatusInCapitals();
    }
}
