namespace Acme.Workforce.Workplace.Tests.FloorPlan.Metric;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Workplace.FloorPlan.Metric;

/// <summary>Checks of metric code, run by the project's runner.</summary>
public static class FallbackFloorPlanMetricReportChecks
{
    public static void FallbackFloorPlanMetricReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1701602045);
        var items = new List<FallbackFloorPlanMetric>();
        for (var index = 0; index < 4; index++)
        {
            items.Add(FallbackFloorPlanMetric.NewFallbackFloorPlanMetric($"fallback-floor-plan-metric-{index}", "sample"));
        }
        var report = FallbackFloorPlanMetricReport.BuildFallbackFloorPlanMetricReport("test", items, clock);
        var counted = 0;
        foreach (var key in report.Counts.Keys.Order(StringComparer.Ordinal))
        {
            var valueValue = report.Counts[key];
            counted += valueValue;
        }
        Check.True(counted <= 4, "counted");
        Check.Equal(report.GeneratedAt, clock.Now(), "generated at");
    }

    public static void RunAll()
    {
        FallbackFloorPlanMetricReportCountsEachRecordOnce();
    }
}
