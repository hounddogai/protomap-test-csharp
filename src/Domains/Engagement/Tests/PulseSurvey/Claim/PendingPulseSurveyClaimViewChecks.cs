namespace Acme.Workforce.Engagement.Tests.PulseSurvey.Claim;

using Acme.Workforce.Engagement.PulseSurvey.Claim;

/// <summary>Checks of claim code, run by the project's runner.</summary>
public static class PendingPulseSurveyClaimViewChecks
{
    public static void PendingPulseSurveyClaimViewShowsTheStatusInCapitals()
    {
        var item = PendingPulseSurveyClaim.NewPendingPulseSurveyClaim("code-9", "name-572");
        var view = PendingPulseSurveyClaimView.ToPendingPulseSurveyClaimView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(PendingPulseSurveyClaimView.ToPendingPulseSurveyClaimViews(new List<PendingPulseSurveyClaim> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        PendingPulseSurveyClaimViewShowsTheStatusInCapitals();
    }
}
