namespace Acme.Workforce.Payrollops.Tests.Payslip.PayslipSnapshot;

using Acme.Workforce.Payrollops.Payslip.PayslipSnapshot;

/// <summary>Checks of payslip snapshot code, run by the project's runner.</summary>
public static class PayslipBatchChecks
{
    public static void PayslipBatchStartsInIdle()
    {
        var item = PayslipBatch.NewPayslipBatch("code-142", "title-759");
        Check.Equal(item.Status, PayslipBatchStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PayslipBatchMovesToDraining()
    {
        var item = PayslipBatch.NewPayslipBatch("code-142", "title-759");
        try
        {
            item.MoveTo(PayslipBatchStatus.Draining);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PayslipBatchStatus.Draining, "status");
    }

    public static void PayslipBatchRejectsMovesFromStopped()
    {
        var item = PayslipBatch.NewPayslipBatch("code-142", "title-759");
        item.Status = PayslipBatchStatus.Stopped;
        Check.Fails(() => item.MoveTo(PayslipBatchStatus.Idle), "move");
    }

    public static void RunAll()
    {
        PayslipBatchStartsInIdle();
        PayslipBatchMovesToDraining();
        PayslipBatchRejectsMovesFromStopped();
    }
}
