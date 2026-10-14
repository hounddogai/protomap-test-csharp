namespace Acme.Workforce.Engagement.Tests.TownHall.Cycle;

using Acme.Workforce.Engagement.TownHall.Cycle;

/// <summary>Checks of cycle code, run by the project's runner.</summary>
public static class TownHallCycleEngineChecks
{
    public static void TownHallCycleEngineKeepsResultsWithinBounds()
    {
        var engine = TownHallCycleEngine.DefaultTownHallCycleEngine();
        var item = TownHallCycle.NewTownHallCycle("code-767", "label-537");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        TownHallCycleEngineKeepsResultsWithinBounds();
    }
}
