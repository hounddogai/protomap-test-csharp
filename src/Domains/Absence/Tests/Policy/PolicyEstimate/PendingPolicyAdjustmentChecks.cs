namespace Acme.Workforce.Absence.Tests.Policy.PolicyEstimate;

using Acme.Workforce.Absence.Policy.PolicyEstimate;

/// <summary>Checks of policy estimate code, run by the project's runner.</summary>
public static class PendingPolicyAdjustmentChecks
{
    public static void PendingPolicyAdjustmentStartsInIdle()
    {
        var item = PendingPolicyAdjustment.NewPendingPolicyAdjustment("code-482", "title-215");
        Check.Equal(item.Status, PendingPolicyAdjustmentStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PendingPolicyAdjustmentMovesToStopped()
    {
        var item = PendingPolicyAdjustment.NewPendingPolicyAdjustment("code-482", "title-215");
        try
        {
            item.MoveTo(PendingPolicyAdjustmentStatus.Stopped);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PendingPolicyAdjustmentStatus.Stopped, "status");
    }

    public static void PendingPolicyAdjustmentRejectsMovesFromStopped()
    {
        var item = PendingPolicyAdjustment.NewPendingPolicyAdjustment("code-482", "title-215");
        item.Status = PendingPolicyAdjustmentStatus.Stopped;
        Check.Fails(() => item.MoveTo(PendingPolicyAdjustmentStatus.Idle), "move");
    }

    public static void RunAll()
    {
        PendingPolicyAdjustmentStartsInIdle();
        PendingPolicyAdjustmentMovesToStopped();
        PendingPolicyAdjustmentRejectsMovesFromStopped();
    }
}
