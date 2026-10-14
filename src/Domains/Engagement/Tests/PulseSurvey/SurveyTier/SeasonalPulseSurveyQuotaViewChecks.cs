namespace Acme.Workforce.Engagement.Tests.PulseSurvey.SurveyTier;

using Acme.Workforce.Engagement.PulseSurvey.SurveyTier;

/// <summary>Checks of survey tier code, run by the project's runner.</summary>
public static class SeasonalPulseSurveyQuotaViewChecks
{
    public static void SeasonalPulseSurveyQuotaViewShowsTheStatusInCapitals()
    {
        var item = SeasonalPulseSurveyQuota.NewSeasonalPulseSurveyQuota("code-535", "label-234");
        var view = SeasonalPulseSurveyQuotaView.ToSeasonalPulseSurveyQuotaView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(SeasonalPulseSurveyQuotaView.ToSeasonalPulseSurveyQuotaViews(new List<SeasonalPulseSurveyQuota> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        SeasonalPulseSurveyQuotaViewShowsTheStatusInCapitals();
    }
}
