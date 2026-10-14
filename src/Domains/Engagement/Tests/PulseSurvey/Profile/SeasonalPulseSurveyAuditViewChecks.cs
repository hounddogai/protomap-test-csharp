namespace Acme.Workforce.Engagement.Tests.PulseSurvey.Profile;

using Acme.Workforce.Engagement.PulseSurvey.Profile;

/// <summary>Checks of profile code, run by the project's runner.</summary>
public static class SeasonalPulseSurveyAuditViewChecks
{
    public static void SeasonalPulseSurveyAuditViewShowsTheStatusInCapitals()
    {
        var item = SeasonalPulseSurveyAudit.NewSeasonalPulseSurveyAudit("code-436", "label-679");
        var view = SeasonalPulseSurveyAuditView.ToSeasonalPulseSurveyAuditView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(SeasonalPulseSurveyAuditView.ToSeasonalPulseSurveyAuditViews(new List<SeasonalPulseSurveyAudit> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        SeasonalPulseSurveyAuditViewShowsTheStatusInCapitals();
    }
}
