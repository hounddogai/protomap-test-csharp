namespace Acme.Workforce.Engagement.Tests.PulseSurvey.Preference;

using Acme.Workforce.Engagement.PulseSurvey.Preference;

/// <summary>Checks of preference code, run by the project's runner.</summary>
public static class DefaultPulseSurveyOverrideViewChecks
{
    public static void DefaultPulseSurveyOverrideViewShowsTheStatusInCapitals()
    {
        var item = DefaultPulseSurveyOverride.NewDefaultPulseSurveyOverride("code-133", "name-793");
        var view = DefaultPulseSurveyOverrideView.ToDefaultPulseSurveyOverrideView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(DefaultPulseSurveyOverrideView.ToDefaultPulseSurveyOverrideViews(new List<DefaultPulseSurveyOverride> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        DefaultPulseSurveyOverrideViewShowsTheStatusInCapitals();
    }
}
