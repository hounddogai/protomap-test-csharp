namespace Acme.Workforce.Orgdesign.Tests.Span.Review;

using Acme.Workforce.Orgdesign.Span.Review;

/// <summary>Checks of review code, run by the project's runner.</summary>
public static class RollingSpanReviewChecks
{
    public static void RollingSpanReviewStartsInOpen()
    {
        var item = RollingSpanReview.NewRollingSpanReview("code-342", "title-667");
        Check.Equal(item.Status, RollingSpanReviewStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void RollingSpanReviewMovesToInReview()
    {
        var item = RollingSpanReview.NewRollingSpanReview("code-342", "title-667");
        try
        {
            item.MoveTo(RollingSpanReviewStatus.InReview);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, RollingSpanReviewStatus.InReview, "status");
    }

    public static void RollingSpanReviewRejectsMovesFromArchived()
    {
        var item = RollingSpanReview.NewRollingSpanReview("code-342", "title-667");
        item.Status = RollingSpanReviewStatus.Archived;
        Check.Fails(() => item.MoveTo(RollingSpanReviewStatus.Open), "move");
    }

    public static void RunAll()
    {
        RollingSpanReviewStartsInOpen();
        RollingSpanReviewMovesToInReview();
        RollingSpanReviewRejectsMovesFromArchived();
    }
}
