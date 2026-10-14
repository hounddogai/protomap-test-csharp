namespace Acme.Workforce.Contractors.Tests.Timesheet.Estimate;

using Acme.Workforce.Contractors.Timesheet.Estimate;

/// <summary>Checks of estimate code, run by the project's runner.</summary>
public static class LegacyTimesheetSummaryViewChecks
{
    public static void LegacyTimesheetSummaryViewShowsTheStatusInCapitals()
    {
        var item = LegacyTimesheetSummary.NewLegacyTimesheetSummary("code-294", "label-1");
        var view = LegacyTimesheetSummaryView.ToLegacyTimesheetSummaryView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(LegacyTimesheetSummaryView.ToLegacyTimesheetSummaryViews(new List<LegacyTimesheetSummary> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        LegacyTimesheetSummaryViewShowsTheStatusInCapitals();
    }
}
