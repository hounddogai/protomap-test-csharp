namespace Acme.Workforce.Engagement.Tests.TownHall.HallTier;

using Acme.Workforce.Engagement.TownHall.HallTier;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of hall tier code, run by the project's runner.</summary>
public static class TownHallBudgetEngineChecks
{
    public static void TownHallBudgetEngineKeepsResultsWithinBounds()
    {
        var engine = TownHallBudgetEngine.DefaultTownHallBudgetEngine(Flags.NewRegistry());
        var item = TownHallBudget.NewTownHallBudget("code-675", "name-770");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        TownHallBudgetEngineKeepsResultsWithinBounds();
    }
}
