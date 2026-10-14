namespace Acme.Workforce.Payrollops.Tests.Payslip.Manifest;

using Acme.Workforce.Payrollops.Payslip.Manifest;

/// <summary>Checks of manifest code, run by the project's runner.</summary>
public static class BatchedPayslipMetricChecks
{
    public static void BatchedPayslipMetricStartsInPlanned()
    {
        var item = BatchedPayslipMetric.NewBatchedPayslipMetric("code-639", "title-10");
        Check.Equal(item.Status, BatchedPayslipMetricStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void BatchedPayslipMetricMovesToInProgress()
    {
        var item = BatchedPayslipMetric.NewBatchedPayslipMetric("code-639", "title-10");
        try
        {
            item.MoveTo(BatchedPayslipMetricStatus.InProgress);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, BatchedPayslipMetricStatus.InProgress, "status");
    }

    public static void BatchedPayslipMetricRejectsMovesFromCancelled()
    {
        var item = BatchedPayslipMetric.NewBatchedPayslipMetric("code-639", "title-10");
        item.Status = BatchedPayslipMetricStatus.Cancelled;
        Check.Fails(() => item.MoveTo(BatchedPayslipMetricStatus.Planned), "move");
    }

    public static void RunAll()
    {
        BatchedPayslipMetricStartsInPlanned();
        BatchedPayslipMetricMovesToInProgress();
        BatchedPayslipMetricRejectsMovesFromCancelled();
    }
}
