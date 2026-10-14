namespace Acme.Workforce.Payrollops.Tests.PayRun.Metric;

using Acme.Workforce.Payrollops.PayRun.Metric;

/// <summary>Checks of metric code, run by the project's runner.</summary>
public static class BatchedPayRunMetricViewChecks
{
    public static void BatchedPayRunMetricViewShowsTheStatusInCapitals()
    {
        var item = BatchedPayRunMetric.NewBatchedPayRunMetric("code-745", "title-405");
        var view = BatchedPayRunMetricView.ToBatchedPayRunMetricView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(BatchedPayRunMetricView.ToBatchedPayRunMetricViews(new List<BatchedPayRunMetric> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        BatchedPayRunMetricViewShowsTheStatusInCapitals();
    }
}
