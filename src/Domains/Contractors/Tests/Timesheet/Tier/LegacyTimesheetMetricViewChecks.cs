namespace Acme.Workforce.Contractors.Tests.Timesheet.Tier;

using Acme.Workforce.Contractors.Timesheet.Tier;

/// <summary>Checks of tier code, run by the project's runner.</summary>
public static class LegacyTimesheetMetricViewChecks
{
    public static void LegacyTimesheetMetricViewShowsTheStatusInCapitals()
    {
        var item = LegacyTimesheetMetric.NewLegacyTimesheetMetric("code-657", "title-76");
        var view = LegacyTimesheetMetricView.ToLegacyTimesheetMetricView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(LegacyTimesheetMetricView.ToLegacyTimesheetMetricViews(new List<LegacyTimesheetMetric> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        LegacyTimesheetMetricViewShowsTheStatusInCapitals();
    }
}
