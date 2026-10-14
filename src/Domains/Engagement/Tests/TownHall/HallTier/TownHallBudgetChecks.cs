namespace Acme.Workforce.Engagement.Tests.TownHall.HallTier;

using Acme.Workforce.Engagement.TownHall.HallTier;

/// <summary>Checks of hall tier code, run by the project's runner.</summary>
public static class TownHallBudgetChecks
{
    public static void TownHallBudgetStartsInIdle()
    {
        var item = TownHallBudget.NewTownHallBudget("code-675", "name-770");
        Check.Equal(item.Status, TownHallBudgetStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void TownHallBudgetMovesToDraining()
    {
        var item = TownHallBudget.NewTownHallBudget("code-675", "name-770");
        try
        {
            item.MoveTo(TownHallBudgetStatus.Draining);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, TownHallBudgetStatus.Draining, "status");
    }

    public static void TownHallBudgetRejectsMovesFromStopped()
    {
        var item = TownHallBudget.NewTownHallBudget("code-675", "name-770");
        item.Status = TownHallBudgetStatus.Stopped;
        Check.Fails(() => item.MoveTo(TownHallBudgetStatus.Idle), "move");
    }

    public static void RunAll()
    {
        TownHallBudgetStartsInIdle();
        TownHallBudgetMovesToDraining();
        TownHallBudgetRejectsMovesFromStopped();
    }
}
