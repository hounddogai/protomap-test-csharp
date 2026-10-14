namespace Acme.Workforce.Engagement.Tests.Feedback.Reservation;

using Acme.Workforce.Engagement.Feedback.Reservation;

/// <summary>Checks of reservation code, run by the project's runner.</summary>
public static class FeedbackVarianceViewChecks
{
    public static void FeedbackVarianceViewShowsTheStatusInCapitals()
    {
        var item = FeedbackVariance.NewFeedbackVariance("code-711", "title-123");
        var view = FeedbackVarianceView.ToFeedbackVarianceView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(FeedbackVarianceView.ToFeedbackVarianceViews(new List<FeedbackVariance> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        FeedbackVarianceViewShowsTheStatusInCapitals();
    }
}
