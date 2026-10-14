namespace Acme.Workforce.Engagement.Tests.Feedback.Estimate;

using Acme.Workforce.Engagement.Feedback.Estimate;

/// <summary>Checks of estimate code, run by the project's runner.</summary>
public static class FeedbackEstimateViewChecks
{
    public static void FeedbackEstimateViewShowsTheStatusInCapitals()
    {
        var item = FeedbackEstimate.NewFeedbackEstimate("code-695", "title-213");
        var view = FeedbackEstimateView.ToFeedbackEstimateView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(FeedbackEstimateView.ToFeedbackEstimateViews(new List<FeedbackEstimate> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        FeedbackEstimateViewShowsTheStatusInCapitals();
    }
}
