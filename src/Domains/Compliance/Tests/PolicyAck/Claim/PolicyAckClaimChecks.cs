namespace Acme.Workforce.Compliance.Tests.PolicyAck.Claim;

using Acme.Workforce.Compliance.PolicyAck.Claim;

/// <summary>Checks of claim code, run by the project's runner.</summary>
public static class PolicyAckClaimChecks
{
    public static void PolicyAckClaimStartsInDraft()
    {
        var item = PolicyAckClaim.NewPolicyAckClaim("code-695", "title-531");
        Check.Equal(item.Status, PolicyAckClaimStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PolicyAckClaimMovesToSuspended()
    {
        var item = PolicyAckClaim.NewPolicyAckClaim("code-695", "title-531");
        try
        {
            item.MoveTo(PolicyAckClaimStatus.Suspended);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PolicyAckClaimStatus.Suspended, "status");
    }

    public static void PolicyAckClaimRejectsMovesFromClosed()
    {
        var item = PolicyAckClaim.NewPolicyAckClaim("code-695", "title-531");
        item.Status = PolicyAckClaimStatus.Closed;
        Check.Fails(() => item.MoveTo(PolicyAckClaimStatus.Draft), "move");
    }

    public static void RunAll()
    {
        PolicyAckClaimStartsInDraft();
        PolicyAckClaimMovesToSuspended();
        PolicyAckClaimRejectsMovesFromClosed();
    }
}
