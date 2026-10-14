namespace Acme.Workforce.Assets.Tests.Refresh.Metric;

using Acme.Workforce.Assets.Refresh.Metric;

/// <summary>Checks of metric code, run by the project's runner.</summary>
public static class BatchedRefreshMetricChecks
{
    public static void BatchedRefreshMetricStartsInPlanned()
    {
        var item = BatchedRefreshMetric.NewBatchedRefreshMetric("code-154", "title-715");
        Check.Equal(item.Status, BatchedRefreshMetricStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void BatchedRefreshMetricMovesToInProgress()
    {
        var item = BatchedRefreshMetric.NewBatchedRefreshMetric("code-154", "title-715");
        try
        {
            item.MoveTo(BatchedRefreshMetricStatus.InProgress);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, BatchedRefreshMetricStatus.InProgress, "status");
    }

    public static void BatchedRefreshMetricRejectsMovesFromCancelled()
    {
        var item = BatchedRefreshMetric.NewBatchedRefreshMetric("code-154", "title-715");
        item.Status = BatchedRefreshMetricStatus.Cancelled;
        Check.Fails(() => item.MoveTo(BatchedRefreshMetricStatus.Planned), "move");
    }

    public static void RunAll()
    {
        BatchedRefreshMetricStartsInPlanned();
        BatchedRefreshMetricMovesToInProgress();
        BatchedRefreshMetricRejectsMovesFromCancelled();
    }
}
