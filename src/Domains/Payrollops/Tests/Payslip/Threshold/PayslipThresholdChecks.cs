namespace Acme.Workforce.Payrollops.Tests.Payslip.Threshold;

using Acme.Workforce.Payrollops.Payslip.Threshold;

/// <summary>Checks of threshold code, run by the project's runner.</summary>
public static class PayslipThresholdChecks
{
    public static void PayslipThresholdStartsInIdle()
    {
        var item = PayslipThreshold.NewPayslipThreshold("code-409", "label-149");
        Check.Equal(item.Status, PayslipThresholdStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PayslipThresholdMovesToDraining()
    {
        var item = PayslipThreshold.NewPayslipThreshold("code-409", "label-149");
        try
        {
            item.MoveTo(PayslipThresholdStatus.Draining);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PayslipThresholdStatus.Draining, "status");
    }

    public static void PayslipThresholdRejectsMovesFromStopped()
    {
        var item = PayslipThreshold.NewPayslipThreshold("code-409", "label-149");
        item.Status = PayslipThresholdStatus.Stopped;
        Check.Fails(() => item.MoveTo(PayslipThresholdStatus.Idle), "move");
    }

    public static void RunAll()
    {
        PayslipThresholdStartsInIdle();
        PayslipThresholdMovesToDraining();
        PayslipThresholdRejectsMovesFromStopped();
    }
}
