namespace Acme.Workforce.Contractors.Tests.Timesheet.TimesheetProfile;

using Acme.Workforce.Contractors.Timesheet.TimesheetProfile;

/// <summary>Checks of timesheet profile code, run by the project's runner.</summary>
public static class LegacyTimesheetProfileViewChecks
{
    public static void LegacyTimesheetProfileViewShowsTheStatusInCapitals()
    {
        var item = LegacyTimesheetProfile.NewLegacyTimesheetProfile("code-868", "title-921");
        var view = LegacyTimesheetProfileView.ToLegacyTimesheetProfileView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(LegacyTimesheetProfileView.ToLegacyTimesheetProfileViews(new List<LegacyTimesheetProfile> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        LegacyTimesheetProfileViewShowsTheStatusInCapitals();
    }
}
