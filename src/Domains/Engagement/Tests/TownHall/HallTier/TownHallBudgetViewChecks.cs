namespace Acme.Workforce.Engagement.Tests.TownHall.HallTier;

using Acme.Workforce.Engagement.TownHall.HallTier;

/// <summary>Checks of hall tier code, run by the project's runner.</summary>
public static class TownHallBudgetViewChecks
{
    public static void TownHallBudgetViewShowsTheStatusInCapitals()
    {
        var item = TownHallBudget.NewTownHallBudget("code-675", "name-770");
        var view = TownHallBudgetView.ToTownHallBudgetView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(TownHallBudgetView.ToTownHallBudgetViews(new List<TownHallBudget> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        TownHallBudgetViewShowsTheStatusInCapitals();
    }
}
