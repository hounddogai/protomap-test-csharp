namespace Acme.Workforce.Workplace.Tests.AccessBadge.Rollup;

using Acme.Workforce.Workplace.AccessBadge.Rollup;

/// <summary>Checks of rollup code, run by the project's runner.</summary>
public static class AccessBadgeDigestEngineChecks
{
    public static void AccessBadgeDigestEngineKeepsResultsWithinBounds()
    {
        var engine = AccessBadgeDigestEngine.DefaultAccessBadgeDigestEngine();
        var item = AccessBadgeDigest.NewAccessBadgeDigest("code-101", "name-800");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        AccessBadgeDigestEngineKeepsResultsWithinBounds();
    }
}
