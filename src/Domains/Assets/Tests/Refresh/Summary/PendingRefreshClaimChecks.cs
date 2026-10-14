namespace Acme.Workforce.Assets.Tests.Refresh.Summary;

using Acme.Workforce.Assets.Refresh.Summary;

/// <summary>Checks of summary code, run by the project's runner.</summary>
public static class PendingRefreshClaimChecks
{
    public static void PendingRefreshClaimStartsInIdle()
    {
        var item = PendingRefreshClaim.NewPendingRefreshClaim("code-247", "title-488");
        Check.Equal(item.Status, PendingRefreshClaimStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PendingRefreshClaimMovesToReady()
    {
        var item = PendingRefreshClaim.NewPendingRefreshClaim("code-247", "title-488");
        try
        {
            item.MoveTo(PendingRefreshClaimStatus.Ready);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PendingRefreshClaimStatus.Ready, "status");
    }

    public static void PendingRefreshClaimRejectsMovesFromStopped()
    {
        var item = PendingRefreshClaim.NewPendingRefreshClaim("code-247", "title-488");
        item.Status = PendingRefreshClaimStatus.Stopped;
        Check.Fails(() => item.MoveTo(PendingRefreshClaimStatus.Idle), "move");
    }

    public static void RunAll()
    {
        PendingRefreshClaimStartsInIdle();
        PendingRefreshClaimMovesToReady();
        PendingRefreshClaimRejectsMovesFromStopped();
    }
}
