namespace Acme.Workforce.Assets.Tests.Refresh.Metric;

using Acme.Workforce.Assets.Refresh.Metric;

/// <summary>Checks of metric code, run by the project's runner.</summary>
public static class BatchedRefreshMetricViewChecks
{
    public static void BatchedRefreshMetricViewShowsTheStatusInCapitals()
    {
        var item = BatchedRefreshMetric.NewBatchedRefreshMetric("code-154", "title-715");
        var view = BatchedRefreshMetricView.ToBatchedRefreshMetricView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(BatchedRefreshMetricView.ToBatchedRefreshMetricViews(new List<BatchedRefreshMetric> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        BatchedRefreshMetricViewShowsTheStatusInCapitals();
    }
}
