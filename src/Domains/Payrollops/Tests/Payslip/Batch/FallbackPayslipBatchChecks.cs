namespace Acme.Workforce.Payrollops.Tests.Payslip.Batch;

using Acme.Workforce.Payrollops.Payslip.Batch;

/// <summary>Checks of batch code, run by the project's runner.</summary>
public static class FallbackPayslipBatchChecks
{
    public static void FallbackPayslipBatchStartsInOpen()
    {
        var item = FallbackPayslipBatch.NewFallbackPayslipBatch("code-371", "name-979");
        Check.Equal(item.Status, FallbackPayslipBatchStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void FallbackPayslipBatchMovesToArchived()
    {
        var item = FallbackPayslipBatch.NewFallbackPayslipBatch("code-371", "name-979");
        try
        {
            item.MoveTo(FallbackPayslipBatchStatus.Archived);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, FallbackPayslipBatchStatus.Archived, "status");
    }

    public static void FallbackPayslipBatchRejectsMovesFromArchived()
    {
        var item = FallbackPayslipBatch.NewFallbackPayslipBatch("code-371", "name-979");
        item.Status = FallbackPayslipBatchStatus.Archived;
        Check.Fails(() => item.MoveTo(FallbackPayslipBatchStatus.Open), "move");
    }

    public static void RunAll()
    {
        FallbackPayslipBatchStartsInOpen();
        FallbackPayslipBatchMovesToArchived();
        FallbackPayslipBatchRejectsMovesFromArchived();
    }
}
