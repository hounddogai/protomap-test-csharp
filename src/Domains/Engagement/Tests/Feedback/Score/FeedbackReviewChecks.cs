namespace Acme.Workforce.Engagement.Tests.Feedback.Score;

using Acme.Workforce.Engagement.Feedback.Score;

/// <summary>Checks of score code, run by the project's runner.</summary>
public static class FeedbackReviewChecks
{
    public static void FeedbackReviewStartsInPlanned()
    {
        var item = FeedbackReview.NewFeedbackReview("code-194", "title-920");
        Check.Equal(item.Status, FeedbackReviewStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void FeedbackReviewMovesToCompleted()
    {
        var item = FeedbackReview.NewFeedbackReview("code-194", "title-920");
        try
        {
            item.MoveTo(FeedbackReviewStatus.Completed);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, FeedbackReviewStatus.Completed, "status");
    }

    public static void FeedbackReviewRejectsMovesFromCancelled()
    {
        var item = FeedbackReview.NewFeedbackReview("code-194", "title-920");
        item.Status = FeedbackReviewStatus.Cancelled;
        Check.Fails(() => item.MoveTo(FeedbackReviewStatus.Planned), "move");
    }

    public static void RunAll()
    {
        FeedbackReviewStartsInPlanned();
        FeedbackReviewMovesToCompleted();
        FeedbackReviewRejectsMovesFromCancelled();
    }
}
