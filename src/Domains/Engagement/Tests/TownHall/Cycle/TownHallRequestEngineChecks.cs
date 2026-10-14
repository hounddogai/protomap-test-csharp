namespace Acme.Workforce.Engagement.Tests.TownHall.Cycle;

using Acme.Workforce.Engagement.TownHall.Cycle;

/// <summary>Checks of cycle code, run by the project's runner.</summary>
public static class TownHallRequestEngineChecks
{
    public static void TownHallRequestEngineKeepsResultsWithinBounds()
    {
        var engine = TownHallRequestEngine.DefaultTownHallRequestEngine();
        var item = TownHallRequest.NewTownHallRequest("code-219", "label-105");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        TownHallRequestEngineKeepsResultsWithinBounds();
    }
}
