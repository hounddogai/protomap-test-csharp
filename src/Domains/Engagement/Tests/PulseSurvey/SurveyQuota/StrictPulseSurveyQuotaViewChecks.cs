namespace Acme.Workforce.Engagement.Tests.PulseSurvey.SurveyQuota;

using Acme.Workforce.Engagement.PulseSurvey.SurveyQuota;

/// <summary>Checks of survey quota code, run by the project's runner.</summary>
public static class StrictPulseSurveyQuotaViewChecks
{
    public static void StrictPulseSurveyQuotaViewShowsTheStatusInCapitals()
    {
        var item = StrictPulseSurveyQuota.NewStrictPulseSurveyQuota("code-449", "name-167");
        var view = StrictPulseSurveyQuotaView.ToStrictPulseSurveyQuotaView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(StrictPulseSurveyQuotaView.ToStrictPulseSurveyQuotaViews(new List<StrictPulseSurveyQuota> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        StrictPulseSurveyQuotaViewShowsTheStatusInCapitals();
    }
}
