namespace Acme.Workforce.Engagement.Tests.PulseSurvey.Profile;

using Acme.Workforce.Engagement.PulseSurvey.Profile;

/// <summary>Checks of profile code, run by the project's runner.</summary>
public static class RollingPulseSurveyCheckpointViewChecks
{
    public static void RollingPulseSurveyCheckpointViewShowsTheStatusInCapitals()
    {
        var item = RollingPulseSurveyCheckpoint.NewRollingPulseSurveyCheckpoint("code-69", "title-188");
        var view = RollingPulseSurveyCheckpointView.ToRollingPulseSurveyCheckpointView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(RollingPulseSurveyCheckpointView.ToRollingPulseSurveyCheckpointViews(new List<RollingPulseSurveyCheckpoint> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        RollingPulseSurveyCheckpointViewShowsTheStatusInCapitals();
    }
}
