namespace Acme.Workforce.Workplace.Tests.AccessBadge.Audit;

using Acme.Workforce.Workplace.AccessBadge.Audit;

/// <summary>Checks of audit code, run by the project's runner.</summary>
public static class AccessBadgeThresholdEngineChecks
{
    public static void AccessBadgeThresholdEngineKeepsResultsWithinBounds()
    {
        var engine = AccessBadgeThresholdEngine.DefaultAccessBadgeThresholdEngine();
        var item = AccessBadgeThreshold.NewAccessBadgeThreshold("code-498", "name-5");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        AccessBadgeThresholdEngineKeepsResultsWithinBounds();
    }
}
