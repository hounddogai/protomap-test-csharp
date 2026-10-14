namespace Acme.Workforce.Payrollops.Tests.PayRun.Metric;

using Acme.Workforce.Payrollops.PayRun.Metric;

/// <summary>Checks of metric code, run by the project's runner.</summary>
public static class BatchedPayRunMetricChecks
{
    public static void BatchedPayRunMetricStartsInRequested()
    {
        var item = BatchedPayRunMetric.NewBatchedPayRunMetric("code-745", "title-405");
        Check.Equal(item.Status, BatchedPayRunMetricStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void BatchedPayRunMetricMovesToGranted()
    {
        var item = BatchedPayRunMetric.NewBatchedPayRunMetric("code-745", "title-405");
        try
        {
            item.MoveTo(BatchedPayRunMetricStatus.Granted);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, BatchedPayRunMetricStatus.Granted, "status");
    }

    public static void BatchedPayRunMetricRejectsMovesFromExpired()
    {
        var item = BatchedPayRunMetric.NewBatchedPayRunMetric("code-745", "title-405");
        item.Status = BatchedPayRunMetricStatus.Expired;
        Check.Fails(() => item.MoveTo(BatchedPayRunMetricStatus.Requested), "move");
    }

    public static void RunAll()
    {
        BatchedPayRunMetricStartsInRequested();
        BatchedPayRunMetricMovesToGranted();
        BatchedPayRunMetricRejectsMovesFromExpired();
    }
}
