namespace Acme.Workforce.Compliance.Tests.PolicyAck.Group;

using Acme.Workforce.Compliance.PolicyAck.Group;

/// <summary>Checks of group code, run by the project's runner.</summary>
public static class PolicyAckGroupChecks
{
    public static void PolicyAckGroupStartsInProposed()
    {
        var item = PolicyAckGroup.NewPolicyAckGroup("code-712", "title-722");
        Check.Equal(item.Status, PolicyAckGroupStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PolicyAckGroupMovesToRejected()
    {
        var item = PolicyAckGroup.NewPolicyAckGroup("code-712", "title-722");
        try
        {
            item.MoveTo(PolicyAckGroupStatus.Rejected);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PolicyAckGroupStatus.Rejected, "status");
    }

    public static void PolicyAckGroupRejectsMovesFromWithdrawn()
    {
        var item = PolicyAckGroup.NewPolicyAckGroup("code-712", "title-722");
        item.Status = PolicyAckGroupStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(PolicyAckGroupStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        PolicyAckGroupStartsInProposed();
        PolicyAckGroupMovesToRejected();
        PolicyAckGroupRejectsMovesFromWithdrawn();
    }
}
