namespace Acme.Workforce.Payrollops.Tests.Payslip.Checkpoint;

using Acme.Workforce.Payrollops.Payslip.Checkpoint;

/// <summary>Checks of checkpoint code, run by the project's runner.</summary>
public static class CappedPayslipBatchChecks
{
    public static void CappedPayslipBatchStartsInIdle()
    {
        var item = CappedPayslipBatch.NewCappedPayslipBatch("code-23", "label-81");
        Check.Equal(item.Status, CappedPayslipBatchStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void CappedPayslipBatchMovesToDraining()
    {
        var item = CappedPayslipBatch.NewCappedPayslipBatch("code-23", "label-81");
        try
        {
            item.MoveTo(CappedPayslipBatchStatus.Draining);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, CappedPayslipBatchStatus.Draining, "status");
    }

    public static void CappedPayslipBatchRejectsMovesFromStopped()
    {
        var item = CappedPayslipBatch.NewCappedPayslipBatch("code-23", "label-81");
        item.Status = CappedPayslipBatchStatus.Stopped;
        Check.Fails(() => item.MoveTo(CappedPayslipBatchStatus.Idle), "move");
    }

    public static void RunAll()
    {
        CappedPayslipBatchStartsInIdle();
        CappedPayslipBatchMovesToDraining();
        CappedPayslipBatchRejectsMovesFromStopped();
    }
}
