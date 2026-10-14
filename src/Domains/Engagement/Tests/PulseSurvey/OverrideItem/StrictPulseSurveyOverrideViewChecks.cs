namespace Acme.Workforce.Engagement.Tests.PulseSurvey.OverrideItem;

using Acme.Workforce.Engagement.PulseSurvey.OverrideItem;

/// <summary>Checks of override item code, run by the project's runner.</summary>
public static class StrictPulseSurveyOverrideViewChecks
{
    public static void StrictPulseSurveyOverrideViewShowsTheStatusInCapitals()
    {
        var item = StrictPulseSurveyOverride.NewStrictPulseSurveyOverride("code-805", "label-768");
        var view = StrictPulseSurveyOverrideView.ToStrictPulseSurveyOverrideView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(StrictPulseSurveyOverrideView.ToStrictPulseSurveyOverrideViews(new List<StrictPulseSurveyOverride> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        StrictPulseSurveyOverrideViewShowsTheStatusInCapitals();
    }
}
