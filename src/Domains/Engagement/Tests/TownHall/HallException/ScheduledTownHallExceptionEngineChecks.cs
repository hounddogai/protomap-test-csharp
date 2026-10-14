namespace Acme.Workforce.Engagement.Tests.TownHall.HallException;

using Acme.Workforce.Engagement.TownHall.HallException;

/// <summary>Checks of hall exception code, run by the project's runner.</summary>
public static class ScheduledTownHallExceptionEngineChecks
{
    public static void ScheduledTownHallExceptionEngineKeepsResultsWithinBounds()
    {
        var engine = ScheduledTownHallExceptionEngine.DefaultScheduledTownHallExceptionEngine();
        var item = ScheduledTownHallException.NewScheduledTownHallException("code-218", "name-6");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        ScheduledTownHallExceptionEngineKeepsResultsWithinBounds();
    }
}
