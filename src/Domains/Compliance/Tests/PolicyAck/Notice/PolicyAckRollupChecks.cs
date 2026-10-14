namespace Acme.Workforce.Compliance.Tests.PolicyAck.Notice;

using Acme.Workforce.Compliance.PolicyAck.Notice;

/// <summary>Checks of notice code, run by the project's runner.</summary>
public static class PolicyAckRollupChecks
{
    public static void PolicyAckRollupStartsInPlanned()
    {
        var item = PolicyAckRollup.NewPolicyAckRollup("code-201", "name-370");
        Check.Equal(item.Status, PolicyAckRollupStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PolicyAckRollupMovesToScheduled()
    {
        var item = PolicyAckRollup.NewPolicyAckRollup("code-201", "name-370");
        try
        {
            item.MoveTo(PolicyAckRollupStatus.Scheduled);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PolicyAckRollupStatus.Scheduled, "status");
    }

    public static void PolicyAckRollupRejectsMovesFromCancelled()
    {
        var item = PolicyAckRollup.NewPolicyAckRollup("code-201", "name-370");
        item.Status = PolicyAckRollupStatus.Cancelled;
        Check.Fails(() => item.MoveTo(PolicyAckRollupStatus.Planned), "move");
    }

    public static void RunAll()
    {
        PolicyAckRollupStartsInPlanned();
        PolicyAckRollupMovesToScheduled();
        PolicyAckRollupRejectsMovesFromCancelled();
    }
}
