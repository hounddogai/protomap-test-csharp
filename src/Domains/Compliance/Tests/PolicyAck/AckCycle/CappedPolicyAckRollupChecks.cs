namespace Acme.Workforce.Compliance.Tests.PolicyAck.AckCycle;

using Acme.Workforce.Compliance.PolicyAck.AckCycle;

/// <summary>Checks of ack cycle code, run by the project's runner.</summary>
public static class CappedPolicyAckRollupChecks
{
    public static void CappedPolicyAckRollupStartsInCreated()
    {
        var item = CappedPolicyAckRollup.NewCappedPolicyAckRollup("code-816", "title-300");
        Check.Equal(item.Status, CappedPolicyAckRollupStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void CappedPolicyAckRollupMovesToPublished()
    {
        var item = CappedPolicyAckRollup.NewCappedPolicyAckRollup("code-816", "title-300");
        try
        {
            item.MoveTo(CappedPolicyAckRollupStatus.Published);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, CappedPolicyAckRollupStatus.Published, "status");
    }

    public static void CappedPolicyAckRollupRejectsMovesFromRetired()
    {
        var item = CappedPolicyAckRollup.NewCappedPolicyAckRollup("code-816", "title-300");
        item.Status = CappedPolicyAckRollupStatus.Retired;
        Check.Fails(() => item.MoveTo(CappedPolicyAckRollupStatus.Created), "move");
    }

    public static void RunAll()
    {
        CappedPolicyAckRollupStartsInCreated();
        CappedPolicyAckRollupMovesToPublished();
        CappedPolicyAckRollupRejectsMovesFromRetired();
    }
}
