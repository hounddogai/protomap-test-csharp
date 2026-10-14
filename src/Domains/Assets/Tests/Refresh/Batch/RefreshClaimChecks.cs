namespace Acme.Workforce.Assets.Tests.Refresh.Batch;

using Acme.Workforce.Assets.Refresh.Batch;

/// <summary>Checks of batch code, run by the project's runner.</summary>
public static class RefreshClaimChecks
{
    public static void RefreshClaimStartsInProposed()
    {
        var item = RefreshClaim.NewRefreshClaim("code-450", "title-953");
        Check.Equal(item.Status, RefreshClaimStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void RefreshClaimMovesToApproved()
    {
        var item = RefreshClaim.NewRefreshClaim("code-450", "title-953");
        try
        {
            item.MoveTo(RefreshClaimStatus.Approved);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, RefreshClaimStatus.Approved, "status");
    }

    public static void RefreshClaimRejectsMovesFromWithdrawn()
    {
        var item = RefreshClaim.NewRefreshClaim("code-450", "title-953");
        item.Status = RefreshClaimStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(RefreshClaimStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        RefreshClaimStartsInProposed();
        RefreshClaimMovesToApproved();
        RefreshClaimRejectsMovesFromWithdrawn();
    }
}
