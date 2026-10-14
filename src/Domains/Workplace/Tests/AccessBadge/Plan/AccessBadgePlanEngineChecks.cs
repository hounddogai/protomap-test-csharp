namespace Acme.Workforce.Workplace.Tests.AccessBadge.Plan;

using Acme.Workforce.Workplace.AccessBadge.Plan;

/// <summary>Checks of plan code, run by the project's runner.</summary>
public static class AccessBadgePlanEngineChecks
{
    public static void AccessBadgePlanEngineKeepsResultsWithinBounds()
    {
        var engine = AccessBadgePlanEngine.DefaultAccessBadgePlanEngine();
        var item = AccessBadgePlan.NewAccessBadgePlan("code-720", "name-277");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        AccessBadgePlanEngineKeepsResultsWithinBounds();
    }
}
