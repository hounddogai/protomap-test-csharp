namespace Acme.Workforce.Workplace.Tests.FloorPlan.Baseline;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Workplace.FloorPlan.Baseline;

/// <summary>Checks of baseline code, run by the project's runner.</summary>
public static class FloorPlanEstimateReportChecks
{
    public static void FloorPlanEstimateReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1706578714);
        var items = new List<FloorPlanEstimate>();
        for (var index = 0; index < 4; index++)
        {
            items.Add(FloorPlanEstimate.NewFloorPlanEstimate($"floor-plan-estimate-{index}", "sample"));
        }
        var report = FloorPlanEstimateReport.BuildFloorPlanEstimateReport("test", items, clock);
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
        FloorPlanEstimateReportCountsEachRecordOnce();
    }
}
