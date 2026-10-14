namespace Acme.Workforce.Orgdesign.Tests.Span.SpanReview;

using Acme.Workforce.Orgdesign.Span.SpanReview;

/// <summary>Checks of span review code, run by the project's runner.</summary>
public static class RegionalSpanReviewChecks
{
    public static void RegionalSpanReviewStartsInRequested()
    {
        var item = RegionalSpanReview.NewRegionalSpanReview("code-185", "label-80");
        Check.Equal(item.Status, RegionalSpanReviewStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void RegionalSpanReviewMovesToRevoked()
    {
        var item = RegionalSpanReview.NewRegionalSpanReview("code-185", "label-80");
        try
        {
            item.MoveTo(RegionalSpanReviewStatus.Revoked);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, RegionalSpanReviewStatus.Revoked, "status");
    }

    public static void RegionalSpanReviewRejectsMovesFromExpired()
    {
        var item = RegionalSpanReview.NewRegionalSpanReview("code-185", "label-80");
        item.Status = RegionalSpanReviewStatus.Expired;
        Check.Fails(() => item.MoveTo(RegionalSpanReviewStatus.Requested), "move");
    }

    public static void RunAll()
    {
        RegionalSpanReviewStartsInRequested();
        RegionalSpanReviewMovesToRevoked();
        RegionalSpanReviewRejectsMovesFromExpired();
    }
}
