namespace Acme.Workforce.Engagement.Tests.TownHall.Cycle;

using Acme.Workforce.Engagement.TownHall.Cycle;

/// <summary>Checks of cycle code, run by the project's runner.</summary>
public static class TownHallCycleViewChecks
{
    public static void TownHallCycleViewShowsTheStatusInCapitals()
    {
        var item = TownHallCycle.NewTownHallCycle("code-767", "label-537");
        var view = TownHallCycleView.ToTownHallCycleView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(TownHallCycleView.ToTownHallCycleViews(new List<TownHallCycle> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        TownHallCycleViewShowsTheStatusInCapitals();
    }
}
