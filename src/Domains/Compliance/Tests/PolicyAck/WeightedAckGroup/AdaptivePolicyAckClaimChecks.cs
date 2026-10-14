namespace Acme.Workforce.Compliance.Tests.PolicyAck.WeightedAckGroup;

using Acme.Workforce.Compliance.PolicyAck.WeightedAckGroup;

/// <summary>Checks of weighted ack group code, run by the project's runner.</summary>
public static class AdaptivePolicyAckClaimChecks
{
    public static void AdaptivePolicyAckClaimStartsInProposed()
    {
        var item = AdaptivePolicyAckClaim.NewAdaptivePolicyAckClaim("code-581", "label-804");
        Check.Equal(item.Status, AdaptivePolicyAckClaimStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void AdaptivePolicyAckClaimMovesToApproved()
    {
        var item = AdaptivePolicyAckClaim.NewAdaptivePolicyAckClaim("code-581", "label-804");
        try
        {
            item.MoveTo(AdaptivePolicyAckClaimStatus.Approved);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, AdaptivePolicyAckClaimStatus.Approved, "status");
    }

    public static void AdaptivePolicyAckClaimRejectsMovesFromWithdrawn()
    {
        var item = AdaptivePolicyAckClaim.NewAdaptivePolicyAckClaim("code-581", "label-804");
        item.Status = AdaptivePolicyAckClaimStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(AdaptivePolicyAckClaimStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        AdaptivePolicyAckClaimStartsInProposed();
        AdaptivePolicyAckClaimMovesToApproved();
        AdaptivePolicyAckClaimRejectsMovesFromWithdrawn();
    }
}
