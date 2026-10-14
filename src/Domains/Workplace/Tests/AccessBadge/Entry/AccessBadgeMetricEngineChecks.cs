namespace Acme.Workforce.Workplace.Tests.AccessBadge.Entry;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Workplace.AccessBadge.Entry;

/// <summary>Checks of entry code, run by the project's runner.</summary>
public static class AccessBadgeMetricEngineChecks
{
    public static void AccessBadgeMetricEngineKeepsResultsWithinBounds()
    {
        var engine = AccessBadgeMetricEngine.DefaultAccessBadgeMetricEngine(Flags.NewRegistry());
        var item = AccessBadgeMetric.NewAccessBadgeMetric("code-173", "label-849");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        AccessBadgeMetricEngineKeepsResultsWithinBounds();
    }
}
