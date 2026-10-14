namespace Acme.Workforce.Engagement.Tests.PulseSurvey.Metric;

using Acme.Workforce.Engagement.PulseSurvey.Metric;

/// <summary>Checks of metric code, run by the project's runner.</summary>
public static class ScheduledPulseSurveyEstimateViewChecks
{
    public static void ScheduledPulseSurveyEstimateViewShowsTheStatusInCapitals()
    {
        var item = ScheduledPulseSurveyEstimate.NewScheduledPulseSurveyEstimate("code-170", "title-555");
        var view = ScheduledPulseSurveyEstimateView.ToScheduledPulseSurveyEstimateView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(ScheduledPulseSurveyEstimateView.ToScheduledPulseSurveyEstimateViews(new List<ScheduledPulseSurveyEstimate> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        ScheduledPulseSurveyEstimateViewShowsTheStatusInCapitals();
    }
}
