namespace Acme.Workforce.Assets.Tests.Refresh.Tier;

using Acme.Workforce.Assets.Refresh.Tier;

/// <summary>Checks of tier code, run by the project's runner.</summary>
public static class LenientRefreshClaimChecks
{
    public static void LenientRefreshClaimStartsInDraft()
    {
        var item = LenientRefreshClaim.NewLenientRefreshClaim("code-996", "label-518");
        Check.Equal(item.Status, LenientRefreshClaimStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void LenientRefreshClaimMovesToPending()
    {
        var item = LenientRefreshClaim.NewLenientRefreshClaim("code-996", "label-518");
        try
        {
            item.MoveTo(LenientRefreshClaimStatus.Pending);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, LenientRefreshClaimStatus.Pending, "status");
    }

    public static void LenientRefreshClaimRejectsMovesFromClosed()
    {
        var item = LenientRefreshClaim.NewLenientRefreshClaim("code-996", "label-518");
        item.Status = LenientRefreshClaimStatus.Closed;
        Check.Fails(() => item.MoveTo(LenientRefreshClaimStatus.Draft), "move");
    }

    public static void RunAll()
    {
        LenientRefreshClaimStartsInDraft();
        LenientRefreshClaimMovesToPending();
        LenientRefreshClaimRejectsMovesFromClosed();
    }
}
