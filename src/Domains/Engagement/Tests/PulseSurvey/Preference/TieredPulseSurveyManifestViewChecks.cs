namespace Acme.Workforce.Engagement.Tests.PulseSurvey.Preference;

using Acme.Workforce.Engagement.PulseSurvey.Preference;

/// <summary>Checks of preference code, run by the project's runner.</summary>
public static class TieredPulseSurveyManifestViewChecks
{
    public static void TieredPulseSurveyManifestViewShowsTheStatusInCapitals()
    {
        var item = TieredPulseSurveyManifest.NewTieredPulseSurveyManifest("code-231", "title-680");
        var view = TieredPulseSurveyManifestView.ToTieredPulseSurveyManifestView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(TieredPulseSurveyManifestView.ToTieredPulseSurveyManifestViews(new List<TieredPulseSurveyManifest> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        TieredPulseSurveyManifestViewShowsTheStatusInCapitals();
    }
}
