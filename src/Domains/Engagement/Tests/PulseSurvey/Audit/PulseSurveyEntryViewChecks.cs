namespace Acme.Workforce.Engagement.Tests.PulseSurvey.Audit;

using Acme.Workforce.Engagement.PulseSurvey.Audit;

/// <summary>Checks of audit code, run by the project's runner.</summary>
public static class PulseSurveyEntryViewChecks
{
    public static void PulseSurveyEntryViewShowsTheStatusInCapitals()
    {
        var item = PulseSurveyEntry.NewPulseSurveyEntry("code-242", "title-420");
        var view = PulseSurveyEntryView.ToPulseSurveyEntryView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(PulseSurveyEntryView.ToPulseSurveyEntryViews(new List<PulseSurveyEntry> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        PulseSurveyEntryViewShowsTheStatusInCapitals();
    }
}
