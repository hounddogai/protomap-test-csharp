namespace Acme.Workforce.Compliance.Tests.PolicyAck.Exception;

using Acme.Workforce.Compliance.PolicyAck.Exception;

/// <summary>Checks of exception code, run by the project's runner.</summary>
public static class PolicyAckRequestChecks
{
    public static void PolicyAckRequestStartsInProposed()
    {
        var item = PolicyAckRequest.NewPolicyAckRequest("code-606", "title-716");
        Check.Equal(item.Status, PolicyAckRequestStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PolicyAckRequestMovesToRejected()
    {
        var item = PolicyAckRequest.NewPolicyAckRequest("code-606", "title-716");
        try
        {
            item.MoveTo(PolicyAckRequestStatus.Rejected);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PolicyAckRequestStatus.Rejected, "status");
    }

    public static void PolicyAckRequestRejectsMovesFromWithdrawn()
    {
        var item = PolicyAckRequest.NewPolicyAckRequest("code-606", "title-716");
        item.Status = PolicyAckRequestStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(PolicyAckRequestStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        PolicyAckRequestStartsInProposed();
        PolicyAckRequestMovesToRejected();
        PolicyAckRequestRejectsMovesFromWithdrawn();
    }
}
