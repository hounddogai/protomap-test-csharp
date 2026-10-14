namespace Acme.Workforce.Contractors.Tests.Timesheet.Preference;

using Acme.Workforce.Contractors.Timesheet.Preference;

/// <summary>Checks of preference code, run by the project's runner.</summary>
public static class PendingTimesheetMetricViewChecks
{
    public static void PendingTimesheetMetricViewShowsTheStatusInCapitals()
    {
        var item = PendingTimesheetMetric.NewPendingTimesheetMetric("code-342", "label-624");
        var view = PendingTimesheetMetricView.ToPendingTimesheetMetricView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(PendingTimesheetMetricView.ToPendingTimesheetMetricViews(new List<PendingTimesheetMetric> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        PendingTimesheetMetricViewShowsTheStatusInCapitals();
    }
}
