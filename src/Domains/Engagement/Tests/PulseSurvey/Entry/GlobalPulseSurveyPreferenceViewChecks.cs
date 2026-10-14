namespace Acme.Workforce.Engagement.Tests.PulseSurvey.Entry;

using Acme.Workforce.Engagement.PulseSurvey.Entry;

/// <summary>Checks of entry code, run by the project's runner.</summary>
public static class GlobalPulseSurveyPreferenceViewChecks
{
    public static void GlobalPulseSurveyPreferenceViewShowsTheStatusInCapitals()
    {
        var item = GlobalPulseSurveyPreference.NewGlobalPulseSurveyPreference("code-778", "title-778");
        var view = GlobalPulseSurveyPreferenceView.ToGlobalPulseSurveyPreferenceView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(GlobalPulseSurveyPreferenceView.ToGlobalPulseSurveyPreferenceViews(new List<GlobalPulseSurveyPreference> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        GlobalPulseSurveyPreferenceViewShowsTheStatusInCapitals();
    }
}
