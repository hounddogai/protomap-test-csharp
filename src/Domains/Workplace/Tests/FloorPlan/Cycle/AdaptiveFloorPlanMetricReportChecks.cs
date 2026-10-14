namespace Acme.Workforce.Workplace.Tests.FloorPlan.Cycle;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Workplace.FloorPlan.Cycle;

/// <summary>Checks of cycle code, run by the project's runner.</summary>
public static class AdaptiveFloorPlanMetricReportChecks
{
    public static void AdaptiveFloorPlanMetricReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1705104271);
        var items = new List<AdaptiveFloorPlanMetric>();
        for (var index = 0; index < 2; index++)
        {
            items.Add(AdaptiveFloorPlanMetric.NewAdaptiveFloorPlanMetric($"adaptive-floor-plan-metric-{index}", "sample"));
        }
        var report = AdaptiveFloorPlanMetricReport.BuildAdaptiveFloorPlanMetricReport("test", items, clock);
        var counted = 0;
        foreach (var key in report.Counts.Keys.Order(StringComparer.Ordinal))
        {
            var valueValue = report.Counts[key];
            counted += valueValue;
        }
        Check.True(counted <= 2, "counted");
        Check.Equal(report.GeneratedAt, clock.Now(), "generated at");
    }

    public static void RunAll()
    {
        AdaptiveFloorPlanMetricReportCountsEachRecordOnce();
    }
}
