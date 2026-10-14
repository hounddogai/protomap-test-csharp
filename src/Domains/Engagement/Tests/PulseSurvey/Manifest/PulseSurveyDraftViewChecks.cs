namespace Acme.Workforce.Engagement.Tests.PulseSurvey.Manifest;

using Acme.Workforce.Engagement.PulseSurvey.Manifest;

/// <summary>Checks of manifest code, run by the project's runner.</summary>
public static class PulseSurveyDraftViewChecks
{
    public static void PulseSurveyDraftViewShowsTheStatusInCapitals()
    {
        var item = PulseSurveyDraft.NewPulseSurveyDraft("code-340", "name-453");
        var view = PulseSurveyDraftView.ToPulseSurveyDraftView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(PulseSurveyDraftView.ToPulseSurveyDraftViews(new List<PulseSurveyDraft> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        PulseSurveyDraftViewShowsTheStatusInCapitals();
    }
}
