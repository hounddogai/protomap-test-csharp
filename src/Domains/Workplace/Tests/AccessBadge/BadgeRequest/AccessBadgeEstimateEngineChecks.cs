namespace Acme.Workforce.Workplace.Tests.AccessBadge.BadgeRequest;

using Acme.Workforce.Workplace.AccessBadge.BadgeRequest;

/// <summary>Checks of badge request code, run by the project's runner.</summary>
public static class AccessBadgeEstimateEngineChecks
{
    public static void AccessBadgeEstimateEngineKeepsResultsWithinBounds()
    {
        var engine = AccessBadgeEstimateEngine.DefaultAccessBadgeEstimateEngine();
        var item = AccessBadgeEstimate.NewAccessBadgeEstimate("code-224", "title-870");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        AccessBadgeEstimateEngineKeepsResultsWithinBounds();
    }
}
