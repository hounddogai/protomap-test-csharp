namespace Acme.Workforce.Engagement.Tests.TownHall.HallException;

using Acme.Workforce.Engagement.TownHall.HallException;

/// <summary>Checks of hall exception code, run by the project's runner.</summary>
public static class TownHallScoreEngineChecks
{
    public static void TownHallScoreEngineKeepsResultsWithinBounds()
    {
        var engine = TownHallScoreEngine.DefaultTownHallScoreEngine();
        var item = TownHallScore.NewTownHallScore("code-231", "name-473");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        TownHallScoreEngineKeepsResultsWithinBounds();
    }
}
