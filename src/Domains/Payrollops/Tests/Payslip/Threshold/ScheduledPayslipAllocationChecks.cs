namespace Acme.Workforce.Payrollops.Tests.Payslip.Threshold;

using Acme.Workforce.Payrollops.Payslip.Threshold;

/// <summary>Checks of threshold code, run by the project's runner.</summary>
public static class ScheduledPayslipAllocationChecks
{
    public static void ScheduledPayslipAllocationStartsInRequested()
    {
        var item = ScheduledPayslipAllocation.NewScheduledPayslipAllocation("code-583", "label-712");
        Check.Equal(item.Status, ScheduledPayslipAllocationStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void ScheduledPayslipAllocationMovesToRevoked()
    {
        var item = ScheduledPayslipAllocation.NewScheduledPayslipAllocation("code-583", "label-712");
        try
        {
            item.MoveTo(ScheduledPayslipAllocationStatus.Revoked);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, ScheduledPayslipAllocationStatus.Revoked, "status");
    }

    public static void ScheduledPayslipAllocationRejectsMovesFromExpired()
    {
        var item = ScheduledPayslipAllocation.NewScheduledPayslipAllocation("code-583", "label-712");
        item.Status = ScheduledPayslipAllocationStatus.Expired;
        Check.Fails(() => item.MoveTo(ScheduledPayslipAllocationStatus.Requested), "move");
    }

    public static void RunAll()
    {
        ScheduledPayslipAllocationStartsInRequested();
        ScheduledPayslipAllocationMovesToRevoked();
        ScheduledPayslipAllocationRejectsMovesFromExpired();
    }
}
