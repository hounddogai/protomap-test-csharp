namespace Acme.Workforce.Engagement.Tests.Feedback.Reservation;

using Acme.Workforce.Engagement.Feedback.Reservation;

/// <summary>Checks of reservation code, run by the project's runner.</summary>
public static class SeasonalFeedbackClaimViewChecks
{
    public static void SeasonalFeedbackClaimViewShowsTheStatusInCapitals()
    {
        var item = SeasonalFeedbackClaim.NewSeasonalFeedbackClaim("code-619", "title-251");
        var view = SeasonalFeedbackClaimView.ToSeasonalFeedbackClaimView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(SeasonalFeedbackClaimView.ToSeasonalFeedbackClaimViews(new List<SeasonalFeedbackClaim> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        SeasonalFeedbackClaimViewShowsTheStatusInCapitals();
    }
}
