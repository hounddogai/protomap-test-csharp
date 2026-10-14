namespace Acme.Workforce.Engagement.Tests.PulseSurvey.Entry;

using Acme.Workforce.Engagement.PulseSurvey.Entry;

/// <summary>Checks of entry code, run by the project's runner.</summary>
public static class SeasonalPulseSurveyEntryViewChecks
{
    public static void SeasonalPulseSurveyEntryViewShowsTheStatusInCapitals()
    {
        var item = SeasonalPulseSurveyEntry.NewSeasonalPulseSurveyEntry("code-181", "label-858");
        var view = SeasonalPulseSurveyEntryView.ToSeasonalPulseSurveyEntryView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(SeasonalPulseSurveyEntryView.ToSeasonalPulseSurveyEntryViews(new List<SeasonalPulseSurveyEntry> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        SeasonalPulseSurveyEntryViewShowsTheStatusInCapitals();
    }
}
