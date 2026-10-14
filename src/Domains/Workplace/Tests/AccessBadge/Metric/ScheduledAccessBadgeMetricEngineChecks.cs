namespace Acme.Workforce.Workplace.Tests.AccessBadge.Metric;

using Acme.Workforce.Workplace.AccessBadge.Metric;

/// <summary>Checks of metric code, run by the project's runner.</summary>
public static class ScheduledAccessBadgeMetricEngineChecks
{
    public static void ScheduledAccessBadgeMetricEngineKeepsResultsWithinBounds()
    {
        var engine = ScheduledAccessBadgeMetricEngine.DefaultScheduledAccessBadgeMetricEngine();
        var item = ScheduledAccessBadgeMetric.NewScheduledAccessBadgeMetric("code-863", "name-726");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        ScheduledAccessBadgeMetricEngineKeepsResultsWithinBounds();
    }
}
