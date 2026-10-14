namespace Acme.Workforce.Payrollops.Tests.Payslip.Rollup;

using Acme.Workforce.Payrollops.Payslip.Rollup;

/// <summary>Checks of rollup code, run by the project's runner.</summary>
public static class PayslipRollupChecks
{
    public static void PayslipRollupStartsInIdle()
    {
        var item = PayslipRollup.NewPayslipRollup("code-501", "name-732");
        Check.Equal(item.Status, PayslipRollupStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PayslipRollupMovesToStopped()
    {
        var item = PayslipRollup.NewPayslipRollup("code-501", "name-732");
        try
        {
            item.MoveTo(PayslipRollupStatus.Stopped);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PayslipRollupStatus.Stopped, "status");
    }

    public static void PayslipRollupRejectsMovesFromStopped()
    {
        var item = PayslipRollup.NewPayslipRollup("code-501", "name-732");
        item.Status = PayslipRollupStatus.Stopped;
        Check.Fails(() => item.MoveTo(PayslipRollupStatus.Idle), "move");
    }

    public static void RunAll()
    {
        PayslipRollupStartsInIdle();
        PayslipRollupMovesToStopped();
        PayslipRollupRejectsMovesFromStopped();
    }
}
