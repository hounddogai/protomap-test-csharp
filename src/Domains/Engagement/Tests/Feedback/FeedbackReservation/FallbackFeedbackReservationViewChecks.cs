namespace Acme.Workforce.Engagement.Tests.Feedback.FeedbackReservation;

using Acme.Workforce.Engagement.Feedback.FeedbackReservation;

/// <summary>Checks of feedback reservation code, run by the project's runner.</summary>
public static class FallbackFeedbackReservationViewChecks
{
    public static void FallbackFeedbackReservationViewShowsTheStatusInCapitals()
    {
        var item = FallbackFeedbackReservation.NewFallbackFeedbackReservation("code-694", "label-960");
        var view = FallbackFeedbackReservationView.ToFallbackFeedbackReservationView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(FallbackFeedbackReservationView.ToFallbackFeedbackReservationViews(new List<FallbackFeedbackReservation> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        FallbackFeedbackReservationViewShowsTheStatusInCapitals();
    }
}
