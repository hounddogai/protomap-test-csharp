namespace Acme.Workforce.Payrollops.Tests.Payslip.Checkpoint;

using Acme.Workforce.Payrollops.Payslip.Checkpoint;

/// <summary>Checks of checkpoint code, run by the project's runner.</summary>
public static class GlobalPayslipCheckpointChecks
{
    public static void GlobalPayslipCheckpointStartsInRequested()
    {
        var item = GlobalPayslipCheckpoint.NewGlobalPayslipCheckpoint("code-107", "name-675");
        Check.Equal(item.Status, GlobalPayslipCheckpointStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void GlobalPayslipCheckpointMovesToRevoked()
    {
        var item = GlobalPayslipCheckpoint.NewGlobalPayslipCheckpoint("code-107", "name-675");
        try
        {
            item.MoveTo(GlobalPayslipCheckpointStatus.Revoked);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, GlobalPayslipCheckpointStatus.Revoked, "status");
    }

    public static void GlobalPayslipCheckpointRejectsMovesFromExpired()
    {
        var item = GlobalPayslipCheckpoint.NewGlobalPayslipCheckpoint("code-107", "name-675");
        item.Status = GlobalPayslipCheckpointStatus.Expired;
        Check.Fails(() => item.MoveTo(GlobalPayslipCheckpointStatus.Requested), "move");
    }

    public static void RunAll()
    {
        GlobalPayslipCheckpointStartsInRequested();
        GlobalPayslipCheckpointMovesToRevoked();
        GlobalPayslipCheckpointRejectsMovesFromExpired();
    }
}
