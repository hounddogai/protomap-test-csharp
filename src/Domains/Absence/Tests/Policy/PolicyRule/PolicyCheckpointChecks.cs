namespace Acme.Workforce.Absence.Tests.Policy.PolicyRule;

using Acme.Workforce.Absence.Policy.PolicyRule;

/// <summary>Checks of policy rule code, run by the project's runner.</summary>
public static class PolicyCheckpointChecks
{
    public static void PolicyCheckpointStartsInIdle()
    {
        var item = PolicyCheckpoint.NewPolicyCheckpoint("code-980", "label-553");
        Check.Equal(item.Status, PolicyCheckpointStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PolicyCheckpointMovesToReady()
    {
        var item = PolicyCheckpoint.NewPolicyCheckpoint("code-980", "label-553");
        try
        {
            item.MoveTo(PolicyCheckpointStatus.Ready);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PolicyCheckpointStatus.Ready, "status");
    }

    public static void PolicyCheckpointRejectsMovesFromStopped()
    {
        var item = PolicyCheckpoint.NewPolicyCheckpoint("code-980", "label-553");
        item.Status = PolicyCheckpointStatus.Stopped;
        Check.Fails(() => item.MoveTo(PolicyCheckpointStatus.Idle), "move");
    }

    public static void RunAll()
    {
        PolicyCheckpointStartsInIdle();
        PolicyCheckpointMovesToReady();
        PolicyCheckpointRejectsMovesFromStopped();
    }
}
