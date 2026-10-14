namespace Acme.Workforce.Engagement.Tests.TownHall.HallException;

using Acme.Workforce.Engagement.TownHall.HallException;

/// <summary>Checks of hall exception code, run by the project's runner.</summary>
public static class TownHallBaselineEngineChecks
{
    public static void TownHallBaselineEngineKeepsResultsWithinBounds()
    {
        var engine = TownHallBaselineEngine.DefaultTownHallBaselineEngine();
        var item = TownHallBaseline.NewTownHallBaseline("code-809", "name-29");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        TownHallBaselineEngineKeepsResultsWithinBounds();
    }
}
