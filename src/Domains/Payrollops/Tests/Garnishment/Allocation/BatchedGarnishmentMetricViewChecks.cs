namespace Acme.Workforce.Payrollops.Tests.Garnishment.Allocation;

using Acme.Workforce.Payrollops.Garnishment.Allocation;

/// <summary>Checks of allocation code, run by the project's runner.</summary>
public static class BatchedGarnishmentMetricViewChecks
{
    public static void BatchedGarnishmentMetricViewShowsTheStatusInCapitals()
    {
        var item = BatchedGarnishmentMetric.NewBatchedGarnishmentMetric("code-791", "name-181");
        var view = BatchedGarnishmentMetricView.ToBatchedGarnishmentMetricView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(BatchedGarnishmentMetricView.ToBatchedGarnishmentMetricViews(new List<BatchedGarnishmentMetric> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        BatchedGarnishmentMetricViewShowsTheStatusInCapitals();
    }
}
