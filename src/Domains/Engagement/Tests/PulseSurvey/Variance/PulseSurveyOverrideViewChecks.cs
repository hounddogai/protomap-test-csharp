namespace Acme.Workforce.Engagement.Tests.PulseSurvey.Variance;

using Acme.Workforce.Engagement.PulseSurvey.Variance;

/// <summary>Checks of variance code, run by the project's runner.</summary>
public static class PulseSurveyOverrideViewChecks
{
    public static void PulseSurveyOverrideViewShowsTheStatusInCapitals()
    {
        var item = PulseSurveyOverride.NewPulseSurveyOverride("code-232", "name-981");
        var view = PulseSurveyOverrideView.ToPulseSurveyOverrideView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(PulseSurveyOverrideView.ToPulseSurveyOverrideViews(new List<PulseSurveyOverride> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        PulseSurveyOverrideViewShowsTheStatusInCapitals();
    }
}
