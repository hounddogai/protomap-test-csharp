namespace Acme.Workforce.Payrollops.Tests.PayRun.Metric;

using Acme.Workforce.Payrollops.PayRun.Metric;

/// <summary>Checks of metric code, run by the project's runner.</summary>
public static class PayRunMetricViewChecks
{
    public static void PayRunMetricViewShowsTheStatusInCapitals()
    {
        var item = PayRunMetric.NewPayRunMetric("code-130", "title-81");
        var view = PayRunMetricView.ToPayRunMetricView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(PayRunMetricView.ToPayRunMetricViews(new List<PayRunMetric> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        PayRunMetricViewShowsTheStatusInCapitals();
    }
}
