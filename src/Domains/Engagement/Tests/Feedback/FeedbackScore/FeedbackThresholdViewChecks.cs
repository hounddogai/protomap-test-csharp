namespace Acme.Workforce.Engagement.Tests.Feedback.FeedbackScore;

using Acme.Workforce.Engagement.Feedback.FeedbackScore;

/// <summary>Checks of feedback score code, run by the project's runner.</summary>
public static class FeedbackThresholdViewChecks
{
    public static void FeedbackThresholdViewShowsTheStatusInCapitals()
    {
        var item = FeedbackThreshold.NewFeedbackThreshold("code-493", "label-298");
        var view = FeedbackThresholdView.ToFeedbackThresholdView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(FeedbackThresholdView.ToFeedbackThresholdViews(new List<FeedbackThreshold> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        FeedbackThresholdViewShowsTheStatusInCapitals();
    }
}
