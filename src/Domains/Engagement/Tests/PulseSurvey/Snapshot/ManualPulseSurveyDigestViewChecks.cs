namespace Acme.Workforce.Engagement.Tests.PulseSurvey.Snapshot;

using Acme.Workforce.Engagement.PulseSurvey.Snapshot;

/// <summary>Checks of snapshot code, run by the project's runner.</summary>
public static class ManualPulseSurveyDigestViewChecks
{
    public static void ManualPulseSurveyDigestViewShowsTheStatusInCapitals()
    {
        var item = ManualPulseSurveyDigest.NewManualPulseSurveyDigest("code-970", "name-270");
        var view = ManualPulseSurveyDigestView.ToManualPulseSurveyDigestView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(ManualPulseSurveyDigestView.ToManualPulseSurveyDigestViews(new List<ManualPulseSurveyDigest> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        ManualPulseSurveyDigestViewShowsTheStatusInCapitals();
    }
}
