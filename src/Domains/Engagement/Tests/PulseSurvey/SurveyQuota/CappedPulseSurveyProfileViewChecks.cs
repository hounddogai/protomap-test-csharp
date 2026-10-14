namespace Acme.Workforce.Engagement.Tests.PulseSurvey.SurveyQuota;

using Acme.Workforce.Engagement.PulseSurvey.SurveyQuota;

/// <summary>Checks of survey quota code, run by the project's runner.</summary>
public static class CappedPulseSurveyProfileViewChecks
{
    public static void CappedPulseSurveyProfileViewShowsTheStatusInCapitals()
    {
        var item = CappedPulseSurveyProfile.NewCappedPulseSurveyProfile("code-437", "title-372");
        var view = CappedPulseSurveyProfileView.ToCappedPulseSurveyProfileView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(CappedPulseSurveyProfileView.ToCappedPulseSurveyProfileViews(new List<CappedPulseSurveyProfile> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        CappedPulseSurveyProfileViewShowsTheStatusInCapitals();
    }
}
