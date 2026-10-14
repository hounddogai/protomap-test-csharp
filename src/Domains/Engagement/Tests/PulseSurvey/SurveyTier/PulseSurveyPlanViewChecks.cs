namespace Acme.Workforce.Engagement.Tests.PulseSurvey.SurveyTier;

using Acme.Workforce.Engagement.PulseSurvey.SurveyTier;

/// <summary>Checks of survey tier code, run by the project's runner.</summary>
public static class PulseSurveyPlanViewChecks
{
    public static void PulseSurveyPlanViewShowsTheStatusInCapitals()
    {
        var item = PulseSurveyPlan.NewPulseSurveyPlan("code-913", "label-457");
        var view = PulseSurveyPlanView.ToPulseSurveyPlanView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(PulseSurveyPlanView.ToPulseSurveyPlanViews(new List<PulseSurveyPlan> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        PulseSurveyPlanViewShowsTheStatusInCapitals();
    }
}
