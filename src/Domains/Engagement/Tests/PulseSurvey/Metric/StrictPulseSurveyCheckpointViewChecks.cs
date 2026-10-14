namespace Acme.Workforce.Engagement.Tests.PulseSurvey.Metric;

using Acme.Workforce.Engagement.PulseSurvey.Metric;

/// <summary>Checks of metric code, run by the project's runner.</summary>
public static class StrictPulseSurveyCheckpointViewChecks
{
    public static void StrictPulseSurveyCheckpointViewShowsTheStatusInCapitals()
    {
        var item = StrictPulseSurveyCheckpoint.NewStrictPulseSurveyCheckpoint("code-341", "label-921");
        var view = StrictPulseSurveyCheckpointView.ToStrictPulseSurveyCheckpointView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(StrictPulseSurveyCheckpointView.ToStrictPulseSurveyCheckpointViews(new List<StrictPulseSurveyCheckpoint> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        StrictPulseSurveyCheckpointViewShowsTheStatusInCapitals();
    }
}
