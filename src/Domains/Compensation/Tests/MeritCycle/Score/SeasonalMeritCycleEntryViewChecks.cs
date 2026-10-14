namespace Acme.Workforce.Compensation.Tests.MeritCycle.Score;

using Acme.Workforce.Compensation.MeritCycle.Score;

/// <summary>Checks of score code, run by the project's runner.</summary>
public static class SeasonalMeritCycleEntryViewChecks
{
    public static void SeasonalMeritCycleEntryViewShowsTheStatusInCapitals()
    {
        var item = SeasonalMeritCycleEntry.NewSeasonalMeritCycleEntry("code-217", "label-366");
        var view = SeasonalMeritCycleEntryView.ToSeasonalMeritCycleEntryView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(SeasonalMeritCycleEntryView.ToSeasonalMeritCycleEntryViews(new List<SeasonalMeritCycleEntry> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        SeasonalMeritCycleEntryViewShowsTheStatusInCapitals();
    }
}
