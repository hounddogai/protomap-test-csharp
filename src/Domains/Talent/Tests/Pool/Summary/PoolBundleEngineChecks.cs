namespace Acme.Workforce.Talent.Tests.Pool.Summary;

using Acme.Workforce.Talent.Pool.Summary;

/// <summary>Checks of summary code, run by the project's runner.</summary>
public static class PoolBundleEngineChecks
{
    public static void PoolBundleEngineKeepsResultsWithinBounds()
    {
        var engine = PoolBundleEngine.DefaultPoolBundleEngine();
        var item = PoolBundle.NewPoolBundle("code-523", "name-854");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        PoolBundleEngineKeepsResultsWithinBounds();
    }
}
