namespace Acme.Workforce.Contractors.Tests.Timesheet.Group;

using Acme.Workforce.Contractors.Timesheet.Group;

/// <summary>Checks of group code, run by the project's runner.</summary>
public static class RegionalTimesheetProfileViewChecks
{
    public static void RegionalTimesheetProfileViewShowsTheStatusInCapitals()
    {
        var item = RegionalTimesheetProfile.NewRegionalTimesheetProfile("code-824", "name-595");
        var view = RegionalTimesheetProfileView.ToRegionalTimesheetProfileView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(RegionalTimesheetProfileView.ToRegionalTimesheetProfileViews(new List<RegionalTimesheetProfile> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        RegionalTimesheetProfileViewShowsTheStatusInCapitals();
    }
}
