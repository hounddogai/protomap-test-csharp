namespace Acme.Workforce.Engagement.Tests.PulseSurvey.Template;

using Acme.Workforce.Engagement.PulseSurvey.Template;

/// <summary>Checks of template code, run by the project's runner.</summary>
public static class PulseSurveyRollupViewChecks
{
    public static void PulseSurveyRollupViewShowsTheStatusInCapitals()
    {
        var item = PulseSurveyRollup.NewPulseSurveyRollup("code-84", "title-537");
        var view = PulseSurveyRollupView.ToPulseSurveyRollupView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(PulseSurveyRollupView.ToPulseSurveyRollupViews(new List<PulseSurveyRollup> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        PulseSurveyRollupViewShowsTheStatusInCapitals();
    }
}
