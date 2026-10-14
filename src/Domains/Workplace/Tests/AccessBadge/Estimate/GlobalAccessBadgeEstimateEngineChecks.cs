namespace Acme.Workforce.Workplace.Tests.AccessBadge.Estimate;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Workplace.AccessBadge.Estimate;

/// <summary>Checks of estimate code, run by the project's runner.</summary>
public static class GlobalAccessBadgeEstimateEngineChecks
{
    public static void GlobalAccessBadgeEstimateEngineKeepsResultsWithinBounds()
    {
        var engine = GlobalAccessBadgeEstimateEngine.DefaultGlobalAccessBadgeEstimateEngine(Flags.NewRegistry());
        var item = GlobalAccessBadgeEstimate.NewGlobalAccessBadgeEstimate("code-482", "name-172");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        GlobalAccessBadgeEstimateEngineKeepsResultsWithinBounds();
    }
}
