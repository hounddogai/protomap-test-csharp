namespace Acme.Workforce.Engagement.Tests.Feedback.Score;

using Acme.Workforce.Engagement.Feedback.Score;

/// <summary>Checks of score code, run by the project's runner.</summary>
public static class FeedbackReviewViewChecks
{
    public static void FeedbackReviewViewShowsTheStatusInCapitals()
    {
        var item = FeedbackReview.NewFeedbackReview("code-194", "title-920");
        var view = FeedbackReviewView.ToFeedbackReviewView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(FeedbackReviewView.ToFeedbackReviewViews(new List<FeedbackReview> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        FeedbackReviewViewShowsTheStatusInCapitals();
    }
}
