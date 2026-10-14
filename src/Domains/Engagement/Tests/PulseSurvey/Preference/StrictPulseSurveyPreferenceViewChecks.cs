namespace Acme.Workforce.Engagement.Tests.PulseSurvey.Preference;

using Acme.Workforce.Engagement.PulseSurvey.Preference;

/// <summary>Checks of preference code, run by the project's runner.</summary>
public static class StrictPulseSurveyPreferenceViewChecks
{
    public static void StrictPulseSurveyPreferenceViewShowsTheStatusInCapitals()
    {
        var item = StrictPulseSurveyPreference.NewStrictPulseSurveyPreference("code-334", "name-888");
        var view = StrictPulseSurveyPreferenceView.ToStrictPulseSurveyPreferenceView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(StrictPulseSurveyPreferenceView.ToStrictPulseSurveyPreferenceViews(new List<StrictPulseSurveyPreference> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        StrictPulseSurveyPreferenceViewShowsTheStatusInCapitals();
    }
}
