namespace Acme.Workforce.Orgdesign.Tests.Span.Draft;

using Acme.Workforce.Orgdesign.Span.Draft;

/// <summary>Checks of draft code, run by the project's runner.</summary>
public static class SpanClaimChecks
{
    public static void SpanClaimStartsInOpen()
    {
        var item = SpanClaim.NewSpanClaim("code-490", "name-450");
        Check.Equal(item.Status, SpanClaimStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void SpanClaimMovesToInReview()
    {
        var item = SpanClaim.NewSpanClaim("code-490", "name-450");
        try
        {
            item.MoveTo(SpanClaimStatus.InReview);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, SpanClaimStatus.InReview, "status");
    }

    public static void SpanClaimRejectsMovesFromArchived()
    {
        var item = SpanClaim.NewSpanClaim("code-490", "name-450");
        item.Status = SpanClaimStatus.Archived;
        Check.Fails(() => item.MoveTo(SpanClaimStatus.Open), "move");
    }

    public static void RunAll()
    {
        SpanClaimStartsInOpen();
        SpanClaimMovesToInReview();
        SpanClaimRejectsMovesFromArchived();
    }
}
