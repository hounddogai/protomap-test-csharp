namespace Acme.Workforce.Engagement.Tests.PulseSurvey.Assignment;

using Acme.Workforce.Engagement.PulseSurvey.Assignment;

/// <summary>Checks of assignment code, run by the project's runner.</summary>
public static class PulseSurveyScoreViewChecks
{
    public static void PulseSurveyScoreViewShowsTheStatusInCapitals()
    {
        var item = PulseSurveyScore.NewPulseSurveyScore("code-514", "name-829");
        var view = PulseSurveyScoreView.ToPulseSurveyScoreView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(PulseSurveyScoreView.ToPulseSurveyScoreViews(new List<PulseSurveyScore> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        PulseSurveyScoreViewShowsTheStatusInCapitals();
    }
}
