namespace Acme.Workforce.Engagement.Tests.TownHall.Adjustment;

using Acme.Workforce.Engagement.TownHall.Adjustment;

/// <summary>Checks of adjustment code, run by the project's runner.</summary>
public static class SeasonalTownHallCycleViewChecks
{
    public static void SeasonalTownHallCycleViewShowsTheStatusInCapitals()
    {
        var item = SeasonalTownHallCycle.NewSeasonalTownHallCycle("code-321", "label-761");
        var view = SeasonalTownHallCycleView.ToSeasonalTownHallCycleView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(SeasonalTownHallCycleView.ToSeasonalTownHallCycleViews(new List<SeasonalTownHallCycle> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        SeasonalTownHallCycleViewShowsTheStatusInCapitals();
    }
}
