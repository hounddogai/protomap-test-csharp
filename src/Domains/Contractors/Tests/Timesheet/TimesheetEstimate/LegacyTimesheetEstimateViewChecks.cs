namespace Acme.Workforce.Contractors.Tests.Timesheet.TimesheetEstimate;

using Acme.Workforce.Contractors.Timesheet.TimesheetEstimate;

/// <summary>Checks of timesheet estimate code, run by the project's runner.</summary>
public static class LegacyTimesheetEstimateViewChecks
{
    public static void LegacyTimesheetEstimateViewShowsTheStatusInCapitals()
    {
        var item = LegacyTimesheetEstimate.NewLegacyTimesheetEstimate("code-889", "title-474");
        var view = LegacyTimesheetEstimateView.ToLegacyTimesheetEstimateView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(LegacyTimesheetEstimateView.ToLegacyTimesheetEstimateViews(new List<LegacyTimesheetEstimate> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        LegacyTimesheetEstimateViewShowsTheStatusInCapitals();
    }
}
