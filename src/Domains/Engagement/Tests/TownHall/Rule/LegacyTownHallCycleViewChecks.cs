namespace Acme.Workforce.Engagement.Tests.TownHall.Rule;

using Acme.Workforce.Engagement.TownHall.Rule;

/// <summary>Checks of rule code, run by the project's runner.</summary>
public static class LegacyTownHallCycleViewChecks
{
    public static void LegacyTownHallCycleViewShowsTheStatusInCapitals()
    {
        var item = LegacyTownHallCycle.NewLegacyTownHallCycle("code-873", "title-319");
        var view = LegacyTownHallCycleView.ToLegacyTownHallCycleView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(LegacyTownHallCycleView.ToLegacyTownHallCycleViews(new List<LegacyTownHallCycle> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        LegacyTownHallCycleViewShowsTheStatusInCapitals();
    }
}
