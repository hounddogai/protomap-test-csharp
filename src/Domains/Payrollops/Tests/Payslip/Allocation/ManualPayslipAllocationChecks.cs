namespace Acme.Workforce.Payrollops.Tests.Payslip.Allocation;

using Acme.Workforce.Payrollops.Payslip.Allocation;

/// <summary>Checks of allocation code, run by the project's runner.</summary>
public static class ManualPayslipAllocationChecks
{
    public static void ManualPayslipAllocationStartsInRequested()
    {
        var item = ManualPayslipAllocation.NewManualPayslipAllocation("code-756", "name-132");
        Check.Equal(item.Status, ManualPayslipAllocationStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void ManualPayslipAllocationMovesToRevoked()
    {
        var item = ManualPayslipAllocation.NewManualPayslipAllocation("code-756", "name-132");
        try
        {
            item.MoveTo(ManualPayslipAllocationStatus.Revoked);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, ManualPayslipAllocationStatus.Revoked, "status");
    }

    public static void ManualPayslipAllocationRejectsMovesFromExpired()
    {
        var item = ManualPayslipAllocation.NewManualPayslipAllocation("code-756", "name-132");
        item.Status = ManualPayslipAllocationStatus.Expired;
        Check.Fails(() => item.MoveTo(ManualPayslipAllocationStatus.Requested), "move");
    }

    public static void RunAll()
    {
        ManualPayslipAllocationStartsInRequested();
        ManualPayslipAllocationMovesToRevoked();
        ManualPayslipAllocationRejectsMovesFromExpired();
    }
}
