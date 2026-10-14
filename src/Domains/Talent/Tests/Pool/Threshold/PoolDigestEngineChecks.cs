namespace Acme.Workforce.Talent.Tests.Pool.Threshold;

using Acme.Workforce.Talent.Pool.Threshold;

/// <summary>Checks of threshold code, run by the project's runner.</summary>
public static class PoolDigestEngineChecks
{
    public static void PoolDigestEngineKeepsResultsWithinBounds()
    {
        var engine = PoolDigestEngine.DefaultPoolDigestEngine();
        var item = PoolDigest.NewPoolDigest("code-920", "title-444");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        PoolDigestEngineKeepsResultsWithinBounds();
    }
}
