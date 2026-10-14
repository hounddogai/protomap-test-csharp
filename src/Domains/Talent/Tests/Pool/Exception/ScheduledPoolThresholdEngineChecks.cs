namespace Acme.Workforce.Talent.Tests.Pool.Exception;

using Acme.Workforce.Talent.Pool.Exception;

/// <summary>Checks of exception code, run by the project's runner.</summary>
public static class ScheduledPoolThresholdEngineChecks
{
    public static void ScheduledPoolThresholdEngineKeepsResultsWithinBounds()
    {
        var engine = ScheduledPoolThresholdEngine.DefaultScheduledPoolThresholdEngine();
        var item = ScheduledPoolThreshold.NewScheduledPoolThreshold("code-749", "title-268");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        ScheduledPoolThresholdEngineKeepsResultsWithinBounds();
    }
}
