namespace Acme.Workforce.Payrollops.Tests.Garnishment.Allocation;

using Acme.Workforce.Payrollops.Garnishment.Allocation;

/// <summary>Checks of allocation code, run by the project's runner.</summary>
public static class BatchedGarnishmentMetricChecks
{
    public static void BatchedGarnishmentMetricStartsInNew()
    {
        var item = BatchedGarnishmentMetric.NewBatchedGarnishmentMetric("code-791", "name-181");
        Check.Equal(item.Status, BatchedGarnishmentMetricStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void BatchedGarnishmentMetricMovesToQueued()
    {
        var item = BatchedGarnishmentMetric.NewBatchedGarnishmentMetric("code-791", "name-181");
        try
        {
            item.MoveTo(BatchedGarnishmentMetricStatus.Queued);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, BatchedGarnishmentMetricStatus.Queued, "status");
    }

    public static void BatchedGarnishmentMetricRejectsMovesFromFailed()
    {
        var item = BatchedGarnishmentMetric.NewBatchedGarnishmentMetric("code-791", "name-181");
        item.Status = BatchedGarnishmentMetricStatus.Failed;
        Check.Fails(() => item.MoveTo(BatchedGarnishmentMetricStatus.New), "move");
    }

    public static void RunAll()
    {
        BatchedGarnishmentMetricStartsInNew();
        BatchedGarnishmentMetricMovesToQueued();
        BatchedGarnishmentMetricRejectsMovesFromFailed();
    }
}
