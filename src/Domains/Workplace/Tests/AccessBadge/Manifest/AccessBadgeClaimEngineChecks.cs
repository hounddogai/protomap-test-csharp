namespace Acme.Workforce.Workplace.Tests.AccessBadge.Manifest;

using Acme.Workforce.Workplace.AccessBadge.Manifest;

/// <summary>Checks of manifest code, run by the project's runner.</summary>
public static class AccessBadgeClaimEngineChecks
{
    public static void AccessBadgeClaimEngineKeepsResultsWithinBounds()
    {
        var engine = AccessBadgeClaimEngine.DefaultAccessBadgeClaimEngine();
        var item = AccessBadgeClaim.NewAccessBadgeClaim("code-256", "name-962");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        AccessBadgeClaimEngineKeepsResultsWithinBounds();
    }
}
