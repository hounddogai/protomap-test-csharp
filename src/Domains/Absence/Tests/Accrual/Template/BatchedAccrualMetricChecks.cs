namespace Acme.Workforce.Absence.Tests.Accrual.Template;

using Acme.Workforce.Absence.Accrual.Template;

/// <summary>Checks of template code, run by the project's runner.</summary>
public static class BatchedAccrualMetricChecks
{
    public static void BatchedAccrualMetricStartsInPlanned()
    {
        var item = BatchedAccrualMetric.NewBatchedAccrualMetric("code-250", "label-185");
        Check.Equal(item.Status, BatchedAccrualMetricStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void BatchedAccrualMetricMovesToScheduled()
    {
        var item = BatchedAccrualMetric.NewBatchedAccrualMetric("code-250", "label-185");
        try
        {
            item.MoveTo(BatchedAccrualMetricStatus.Scheduled);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, BatchedAccrualMetricStatus.Scheduled, "status");
    }

    public static void BatchedAccrualMetricRejectsMovesFromCancelled()
    {
        var item = BatchedAccrualMetric.NewBatchedAccrualMetric("code-250", "label-185");
        item.Status = BatchedAccrualMetricStatus.Cancelled;
        Check.Fails(() => item.MoveTo(BatchedAccrualMetricStatus.Planned), "move");
    }

    public static void RunAll()
    {
        BatchedAccrualMetricStartsInPlanned();
        BatchedAccrualMetricMovesToScheduled();
        BatchedAccrualMetricRejectsMovesFromCancelled();
    }
}
