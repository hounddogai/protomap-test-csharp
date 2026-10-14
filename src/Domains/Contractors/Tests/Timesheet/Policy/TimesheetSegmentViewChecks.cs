namespace Acme.Workforce.Contractors.Tests.Timesheet.Policy;

using Acme.Workforce.Contractors.Timesheet.Policy;

/// <summary>Checks of policy code, run by the project's runner.</summary>
public static class TimesheetSegmentViewChecks
{
    public static void TimesheetSegmentViewShowsTheStatusInCapitals()
    {
        var item = TimesheetSegment.NewTimesheetSegment("code-494", "name-332");
        var view = TimesheetSegmentView.ToTimesheetSegmentView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(TimesheetSegmentView.ToTimesheetSegmentViews(new List<TimesheetSegment> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        TimesheetSegmentViewShowsTheStatusInCapitals();
    }
}
