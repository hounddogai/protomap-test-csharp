namespace Acme.Workforce.Contractors.Tests.Engagement.Notice;

using Acme.Workforce.Contractors.Engagement.Notice;

/// <summary>Checks of notice code, run by the project's runner.</summary>
public static class EngagementClaimChecks
{
    public static void EngagementClaimStartsInProposed()
    {
        var item = EngagementClaim.NewEngagementClaim("code-139", "label-687");
        Check.Equal(item.Status, EngagementClaimStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void EngagementClaimMovesToRejected()
    {
        var item = EngagementClaim.NewEngagementClaim("code-139", "label-687");
        try
        {
            item.MoveTo(EngagementClaimStatus.Rejected);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, EngagementClaimStatus.Rejected, "status");
    }

    public static void EngagementClaimRejectsMovesFromWithdrawn()
    {
        var item = EngagementClaim.NewEngagementClaim("code-139", "label-687");
        item.Status = EngagementClaimStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(EngagementClaimStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        EngagementClaimStartsInProposed();
        EngagementClaimMovesToRejected();
        EngagementClaimRejectsMovesFromWithdrawn();
    }
}
