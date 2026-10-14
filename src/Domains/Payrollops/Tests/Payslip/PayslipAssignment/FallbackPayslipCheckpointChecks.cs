namespace Acme.Workforce.Payrollops.Tests.Payslip.PayslipAssignment;

using Acme.Workforce.Payrollops.Payslip.PayslipAssignment;

/// <summary>Checks of payslip assignment code, run by the project's runner.</summary>
public static class FallbackPayslipCheckpointChecks
{
    public static void FallbackPayslipCheckpointStartsInProposed()
    {
        var item = FallbackPayslipCheckpoint.NewFallbackPayslipCheckpoint("code-48", "name-255");
        Check.Equal(item.Status, FallbackPayslipCheckpointStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void FallbackPayslipCheckpointMovesToApproved()
    {
        var item = FallbackPayslipCheckpoint.NewFallbackPayslipCheckpoint("code-48", "name-255");
        try
        {
            item.MoveTo(FallbackPayslipCheckpointStatus.Approved);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, FallbackPayslipCheckpointStatus.Approved, "status");
    }

    public static void FallbackPayslipCheckpointRejectsMovesFromWithdrawn()
    {
        var item = FallbackPayslipCheckpoint.NewFallbackPayslipCheckpoint("code-48", "name-255");
        item.Status = FallbackPayslipCheckpointStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(FallbackPayslipCheckpointStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        FallbackPayslipCheckpointStartsInProposed();
        FallbackPayslipCheckpointMovesToApproved();
        FallbackPayslipCheckpointRejectsMovesFromWithdrawn();
    }
}
