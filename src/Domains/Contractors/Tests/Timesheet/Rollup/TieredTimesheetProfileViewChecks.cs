namespace Acme.Workforce.Contractors.Tests.Timesheet.Rollup;

using Acme.Workforce.Contractors.Timesheet.Rollup;

/// <summary>Checks of rollup code, run by the project's runner.</summary>
public static class TieredTimesheetProfileViewChecks
{
    public static void TieredTimesheetProfileViewShowsTheStatusInCapitals()
    {
        var item = TieredTimesheetProfile.NewTieredTimesheetProfile("code-298", "title-477");
        var view = TieredTimesheetProfileView.ToTieredTimesheetProfileView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(TieredTimesheetProfileView.ToTieredTimesheetProfileViews(new List<TieredTimesheetProfile> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        TieredTimesheetProfileViewShowsTheStatusInCapitals();
    }
}
