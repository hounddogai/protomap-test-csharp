namespace Acme.Workforce.Compensation.Tests.EquityGrant.Metric;

using Acme.Workforce.Compensation.EquityGrant.Metric;

/// <summary>Checks of metric code, run by the project's runner.</summary>
public static class EquityGrantMetricViewChecks
{
    public static void EquityGrantMetricViewShowsTheStatusInCapitals()
    {
        var item = EquityGrantMetric.NewEquityGrantMetric("code-770", "label-967");
        var view = EquityGrantMetricView.ToEquityGrantMetricView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(EquityGrantMetricView.ToEquityGrantMetricViews(new List<EquityGrantMetric> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        EquityGrantMetricViewShowsTheStatusInCapitals();
    }
}
