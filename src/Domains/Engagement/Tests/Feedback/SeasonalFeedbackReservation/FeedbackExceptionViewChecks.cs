namespace Acme.Workforce.Engagement.Tests.Feedback.SeasonalFeedbackReservation;

using Acme.Workforce.Engagement.Feedback.SeasonalFeedbackReservation;

/// <summary>Checks of seasonal feedback reservation code, run by the project's runner.</summary>
public static class FeedbackExceptionViewChecks
{
    public static void FeedbackExceptionViewShowsTheStatusInCapitals()
    {
        var item = FeedbackException.NewFeedbackException("code-106", "label-939");
        var view = FeedbackExceptionView.ToFeedbackExceptionView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(FeedbackExceptionView.ToFeedbackExceptionViews(new List<FeedbackException> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        FeedbackExceptionViewShowsTheStatusInCapitals();
    }
}
