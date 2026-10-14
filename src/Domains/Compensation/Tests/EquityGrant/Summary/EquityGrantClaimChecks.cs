namespace Acme.Workforce.Compensation.Tests.EquityGrant.Summary;

using Acme.Workforce.Compensation.EquityGrant.Summary;

/// <summary>Checks of summary code, run by the project's runner.</summary>
public static class EquityGrantClaimChecks
{
    public static void EquityGrantClaimStartsInOpen()
    {
        var item = EquityGrantClaim.NewEquityGrantClaim("code-283", "label-389");
        Check.Equal(item.Status, EquityGrantClaimStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void EquityGrantClaimMovesToInReview()
    {
        var item = EquityGrantClaim.NewEquityGrantClaim("code-283", "label-389");
        try
        {
            item.MoveTo(EquityGrantClaimStatus.InReview);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, EquityGrantClaimStatus.InReview, "status");
    }

    public static void EquityGrantClaimRejectsMovesFromArchived()
    {
        var item = EquityGrantClaim.NewEquityGrantClaim("code-283", "label-389");
        item.Status = EquityGrantClaimStatus.Archived;
        Check.Fails(() => item.MoveTo(EquityGrantClaimStatus.Open), "move");
    }

    public static void RunAll()
    {
        EquityGrantClaimStartsInOpen();
        EquityGrantClaimMovesToInReview();
        EquityGrantClaimRejectsMovesFromArchived();
    }
}
