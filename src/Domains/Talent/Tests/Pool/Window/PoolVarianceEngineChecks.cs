namespace Acme.Workforce.Talent.Tests.Pool.Window;

using Acme.Workforce.Talent.Pool.Window;

/// <summary>Checks of window code, run by the project's runner.</summary>
public static class PoolVarianceEngineChecks
{
    public static void PoolVarianceEngineKeepsResultsWithinBounds()
    {
        var engine = PoolVarianceEngine.DefaultPoolVarianceEngine();
        var item = PoolVariance.NewPoolVariance("code-534", "label-899");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        PoolVarianceEngineKeepsResultsWithinBounds();
    }
}
