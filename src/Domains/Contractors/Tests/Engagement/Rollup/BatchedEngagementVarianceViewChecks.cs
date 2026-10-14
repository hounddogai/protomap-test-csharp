namespace Acme.Workforce.Contractors.Tests.Engagement.Rollup;

using Acme.Workforce.Contractors.Engagement.Rollup;

/// <summary>Checks of rollup code, run by the project's runner.</summary>
public static class BatchedEngagementVarianceViewChecks
{
    public static void BatchedEngagementVarianceViewShowsTheStatusInCapitals()
    {
        var item = BatchedEngagementVariance.NewBatchedEngagementVariance("code-615", "label-636");
        var view = BatchedEngagementVarianceView.ToBatchedEngagementVarianceView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(BatchedEngagementVarianceView.ToBatchedEngagementVarianceViews(new List<BatchedEngagementVariance> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        BatchedEngagementVarianceViewShowsTheStatusInCapitals();
    }
}
