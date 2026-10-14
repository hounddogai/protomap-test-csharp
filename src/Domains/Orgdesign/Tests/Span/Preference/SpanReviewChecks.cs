namespace Acme.Workforce.Orgdesign.Tests.Span.Preference;

using Acme.Workforce.Orgdesign.Span.Preference;

/// <summary>Checks of preference code, run by the project's runner.</summary>
public static class SpanReviewChecks
{
    public static void SpanReviewStartsInPlanned()
    {
        var item = SpanReview.NewSpanReview("code-354", "name-160");
        Check.Equal(item.Status, SpanReviewStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void SpanReviewMovesToScheduled()
    {
        var item = SpanReview.NewSpanReview("code-354", "name-160");
        try
        {
            item.MoveTo(SpanReviewStatus.Scheduled);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, SpanReviewStatus.Scheduled, "status");
    }

    public static void SpanReviewRejectsMovesFromCancelled()
    {
        var item = SpanReview.NewSpanReview("code-354", "name-160");
        item.Status = SpanReviewStatus.Cancelled;
        Check.Fails(() => item.MoveTo(SpanReviewStatus.Planned), "move");
    }

    public static void RunAll()
    {
        SpanReviewStartsInPlanned();
        SpanReviewMovesToScheduled();
        SpanReviewRejectsMovesFromCancelled();
    }
}
