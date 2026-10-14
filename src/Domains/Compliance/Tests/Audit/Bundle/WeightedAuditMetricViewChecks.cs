namespace Acme.Workforce.Compliance.Tests.Audit.Bundle;

using Acme.Workforce.Compliance.Audit.Bundle;

/// <summary>Checks of bundle code, run by the project's runner.</summary>
public static class WeightedAuditMetricViewChecks
{
    public static void WeightedAuditMetricViewShowsTheStatusInCapitals()
    {
        var item = WeightedAuditMetric.NewWeightedAuditMetric("code-336", "title-696");
        var view = WeightedAuditMetricView.ToWeightedAuditMetricView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(WeightedAuditMetricView.ToWeightedAuditMetricViews(new List<WeightedAuditMetric> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        WeightedAuditMetricViewShowsTheStatusInCapitals();
    }
}
