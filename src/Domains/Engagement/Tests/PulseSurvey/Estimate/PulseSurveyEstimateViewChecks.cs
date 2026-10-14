namespace Acme.Workforce.Engagement.Tests.PulseSurvey.Estimate;

using Acme.Workforce.Engagement.PulseSurvey.Estimate;

/// <summary>Checks of estimate code, run by the project's runner.</summary>
public static class PulseSurveyEstimateViewChecks
{
    public static void PulseSurveyEstimateViewShowsTheStatusInCapitals()
    {
        var item = PulseSurveyEstimate.NewPulseSurveyEstimate("code-795", "name-705");
        var view = PulseSurveyEstimateView.ToPulseSurveyEstimateView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(PulseSurveyEstimateView.ToPulseSurveyEstimateViews(new List<PulseSurveyEstimate> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        PulseSurveyEstimateViewShowsTheStatusInCapitals();
    }
}
