namespace Acme.Workforce.Engagement.Tests.PulseSurvey.OverrideItem;

using Acme.Workforce.Engagement.PulseSurvey.OverrideItem;

/// <summary>Checks of override item code, run by the project's runner.</summary>
public static class PulseSurveySnapshotViewChecks
{
    public static void PulseSurveySnapshotViewShowsTheStatusInCapitals()
    {
        var item = PulseSurveySnapshot.NewPulseSurveySnapshot("code-221", "title-388");
        var view = PulseSurveySnapshotView.ToPulseSurveySnapshotView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(PulseSurveySnapshotView.ToPulseSurveySnapshotViews(new List<PulseSurveySnapshot> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        PulseSurveySnapshotViewShowsTheStatusInCapitals();
    }
}
