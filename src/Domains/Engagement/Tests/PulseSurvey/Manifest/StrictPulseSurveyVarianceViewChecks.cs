namespace Acme.Workforce.Engagement.Tests.PulseSurvey.Manifest;

using Acme.Workforce.Engagement.PulseSurvey.Manifest;

/// <summary>Checks of manifest code, run by the project's runner.</summary>
public static class StrictPulseSurveyVarianceViewChecks
{
    public static void StrictPulseSurveyVarianceViewShowsTheStatusInCapitals()
    {
        var item = StrictPulseSurveyVariance.NewStrictPulseSurveyVariance("code-104", "title-24");
        var view = StrictPulseSurveyVarianceView.ToStrictPulseSurveyVarianceView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(StrictPulseSurveyVarianceView.ToStrictPulseSurveyVarianceViews(new List<StrictPulseSurveyVariance> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        StrictPulseSurveyVarianceViewShowsTheStatusInCapitals();
    }
}
