namespace Acme.Workforce.Payrollops.Tests.Payslip.Batch;

using Acme.Workforce.Payrollops.Payslip.Batch;

/// <summary>Checks of batch code, run by the project's runner.</summary>
public static class PayslipAdjustmentChecks
{
    public static void PayslipAdjustmentStartsInIdle()
    {
        var item = PayslipAdjustment.NewPayslipAdjustment("code-920", "name-767");
        Check.Equal(item.Status, PayslipAdjustmentStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PayslipAdjustmentMovesToReady()
    {
        var item = PayslipAdjustment.NewPayslipAdjustment("code-920", "name-767");
        try
        {
            item.MoveTo(PayslipAdjustmentStatus.Ready);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PayslipAdjustmentStatus.Ready, "status");
    }

    public static void PayslipAdjustmentRejectsMovesFromStopped()
    {
        var item = PayslipAdjustment.NewPayslipAdjustment("code-920", "name-767");
        item.Status = PayslipAdjustmentStatus.Stopped;
        Check.Fails(() => item.MoveTo(PayslipAdjustmentStatus.Idle), "move");
    }

    public static void RunAll()
    {
        PayslipAdjustmentStartsInIdle();
        PayslipAdjustmentMovesToReady();
        PayslipAdjustmentRejectsMovesFromStopped();
    }
}
