namespace Acme.Workforce.Compliance.Tests.PolicyAck.Cycle;

using Acme.Workforce.Compliance.PolicyAck.Cycle;

/// <summary>Checks of cycle code, run by the project's runner.</summary>
public static class ManualPolicyAckGroupChecks
{
    public static void ManualPolicyAckGroupStartsInIdle()
    {
        var item = ManualPolicyAckGroup.NewManualPolicyAckGroup("code-674", "name-237");
        Check.Equal(item.Status, ManualPolicyAckGroupStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void ManualPolicyAckGroupMovesToWarming()
    {
        var item = ManualPolicyAckGroup.NewManualPolicyAckGroup("code-674", "name-237");
        try
        {
            item.MoveTo(ManualPolicyAckGroupStatus.Warming);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, ManualPolicyAckGroupStatus.Warming, "status");
    }

    public static void ManualPolicyAckGroupRejectsMovesFromStopped()
    {
        var item = ManualPolicyAckGroup.NewManualPolicyAckGroup("code-674", "name-237");
        item.Status = ManualPolicyAckGroupStatus.Stopped;
        Check.Fails(() => item.MoveTo(ManualPolicyAckGroupStatus.Idle), "move");
    }

    public static void RunAll()
    {
        ManualPolicyAckGroupStartsInIdle();
        ManualPolicyAckGroupMovesToWarming();
        ManualPolicyAckGroupRejectsMovesFromStopped();
    }
}
