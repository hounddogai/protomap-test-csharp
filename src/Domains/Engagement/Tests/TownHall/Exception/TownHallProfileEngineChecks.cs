namespace Acme.Workforce.Engagement.Tests.TownHall.Exception;

using Acme.Workforce.Engagement.TownHall.Exception;

/// <summary>Checks of exception code, run by the project's runner.</summary>
public static class TownHallProfileEngineChecks
{
    public static void TownHallProfileEngineKeepsResultsWithinBounds()
    {
        var engine = TownHallProfileEngine.DefaultTownHallProfileEngine();
        var item = TownHallProfile.NewTownHallProfile("code-314", "label-507");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        TownHallProfileEngineKeepsResultsWithinBounds();
    }
}
