namespace Acme.Workforce.Compliance.Tests.Audit.Preference;

using Acme.Workforce.Compliance.Audit.Preference;

/// <summary>Checks of preference code, run by the project's runner.</summary>
public static class StrictAuditMetricViewChecks
{
    public static void StrictAuditMetricViewShowsTheStatusInCapitals()
    {
        var item = StrictAuditMetric.NewStrictAuditMetric("code-304", "label-234");
        var view = StrictAuditMetricView.ToStrictAuditMetricView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(StrictAuditMetricView.ToStrictAuditMetricViews(new List<StrictAuditMetric> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        StrictAuditMetricViewShowsTheStatusInCapitals();
    }
}
