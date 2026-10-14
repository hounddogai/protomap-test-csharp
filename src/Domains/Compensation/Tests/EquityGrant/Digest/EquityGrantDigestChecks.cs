namespace Acme.Workforce.Compensation.Tests.EquityGrant.Digest;

using Acme.Workforce.Compensation.EquityGrant.Digest;

/// <summary>Checks of digest code, run by the project's runner.</summary>
public static class EquityGrantDigestChecks
{
    public static void EquityGrantDigestStartsInOpen()
    {
        var item = EquityGrantDigest.NewEquityGrantDigest("code-785", "label-856");
        Check.Equal(item.Status, EquityGrantDigestStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void EquityGrantDigestMovesToInReview()
    {
        var item = EquityGrantDigest.NewEquityGrantDigest("code-785", "label-856");
        try
        {
            item.MoveTo(EquityGrantDigestStatus.InReview);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, EquityGrantDigestStatus.InReview, "status");
    }

    public static void EquityGrantDigestRejectsMovesFromArchived()
    {
        var item = EquityGrantDigest.NewEquityGrantDigest("code-785", "label-856");
        item.Status = EquityGrantDigestStatus.Archived;
        Check.Fails(() => item.MoveTo(EquityGrantDigestStatus.Open), "move");
    }

    public static void RunAll()
    {
        EquityGrantDigestStartsInOpen();
        EquityGrantDigestMovesToInReview();
        EquityGrantDigestRejectsMovesFromArchived();
    }
}
