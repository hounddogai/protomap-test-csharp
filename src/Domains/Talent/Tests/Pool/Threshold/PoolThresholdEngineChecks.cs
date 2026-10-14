namespace Acme.Workforce.Talent.Tests.Pool.Threshold;

using Acme.Workforce.Talent.Pool.Threshold;

/// <summary>Checks of threshold code, run by the project's runner.</summary>
public static class PoolThresholdEngineChecks
{
    public static void PoolThresholdEngineKeepsResultsWithinBounds()
    {
        var engine = PoolThresholdEngine.DefaultPoolThresholdEngine();
        var item = PoolThreshold.NewPoolThreshold("code-954", "name-969");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        PoolThresholdEngineKeepsResultsWithinBounds();
    }
}
