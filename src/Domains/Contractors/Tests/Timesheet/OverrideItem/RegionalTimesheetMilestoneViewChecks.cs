namespace Acme.Workforce.Contractors.Tests.Timesheet.OverrideItem;

using Acme.Workforce.Contractors.Timesheet.OverrideItem;

/// <summary>Checks of override item code, run by the project's runner.</summary>
public static class RegionalTimesheetMilestoneViewChecks
{
    public static void RegionalTimesheetMilestoneViewShowsTheStatusInCapitals()
    {
        var item = RegionalTimesheetMilestone.NewRegionalTimesheetMilestone("code-240", "name-362");
        var view = RegionalTimesheetMilestoneView.ToRegionalTimesheetMilestoneView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(RegionalTimesheetMilestoneView.ToRegionalTimesheetMilestoneViews(new List<RegionalTimesheetMilestone> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        RegionalTimesheetMilestoneViewShowsTheStatusInCapitals();
    }
}
