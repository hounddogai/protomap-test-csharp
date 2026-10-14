namespace Acme.Workforce.Contractors.Tests.Rate.Request;

using Acme.Workforce.Contractors.Rate.Request;

/// <summary>Checks of request code, run by the project's runner.</summary>
public static class RateClaimChecks
{
    public static void RateClaimStartsInDraft()
    {
        var item = RateClaim.NewRateClaim("code-259", "name-422");
        Check.Equal(item.Status, RateClaimStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void RateClaimMovesToActive()
    {
        var item = RateClaim.NewRateClaim("code-259", "name-422");
        try
        {
            item.MoveTo(RateClaimStatus.Active);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, RateClaimStatus.Active, "status");
    }

    public static void RateClaimRejectsMovesFromClosed()
    {
        var item = RateClaim.NewRateClaim("code-259", "name-422");
        item.Status = RateClaimStatus.Closed;
        Check.Fails(() => item.MoveTo(RateClaimStatus.Draft), "move");
    }

    public static void RunAll()
    {
        RateClaimStartsInDraft();
        RateClaimMovesToActive();
        RateClaimRejectsMovesFromClosed();
    }
}
