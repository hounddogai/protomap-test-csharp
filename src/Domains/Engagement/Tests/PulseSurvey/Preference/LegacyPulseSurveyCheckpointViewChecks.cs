namespace Acme.Workforce.Engagement.Tests.PulseSurvey.Preference;

using Acme.Workforce.Engagement.PulseSurvey.Preference;

/// <summary>Checks of preference code, run by the project's runner.</summary>
public static class LegacyPulseSurveyCheckpointViewChecks
{
    public static void LegacyPulseSurveyCheckpointViewShowsTheStatusInCapitals()
    {
        var item = LegacyPulseSurveyCheckpoint.NewLegacyPulseSurveyCheckpoint("code-583", "label-71");
        var view = LegacyPulseSurveyCheckpointView.ToLegacyPulseSurveyCheckpointView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(LegacyPulseSurveyCheckpointView.ToLegacyPulseSurveyCheckpointViews(new List<LegacyPulseSurveyCheckpoint> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        LegacyPulseSurveyCheckpointViewShowsTheStatusInCapitals();
    }
}
