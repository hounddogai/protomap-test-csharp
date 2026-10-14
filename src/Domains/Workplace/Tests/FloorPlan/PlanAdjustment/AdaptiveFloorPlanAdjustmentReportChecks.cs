namespace Acme.Workforce.Workplace.Tests.FloorPlan.PlanAdjustment;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Workplace.FloorPlan.PlanAdjustment;

/// <summary>Checks of plan adjustment code, run by the project's runner.</summary>
public static class AdaptiveFloorPlanAdjustmentReportChecks
{
    public static void AdaptiveFloorPlanAdjustmentReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1706524753);
        var items = new List<AdaptiveFloorPlanAdjustment>();
        for (var index = 0; index < 5; index++)
        {
            items.Add(AdaptiveFloorPlanAdjustment.NewAdaptiveFloorPlanAdjustment($"adaptive-floor-plan-adjustment-{index}", "sample"));
        }
        var report = AdaptiveFloorPlanAdjustmentReport.BuildAdaptiveFloorPlanAdjustmentReport("test", items, clock);
        var counted = 0;
        foreach (var key in report.Counts.Keys.Order(StringComparer.Ordinal))
        {
            var valueValue = report.Counts[key];
            counted += valueValue;
        }
        Check.True(counted <= 5, "counted");
        Check.Equal(report.GeneratedAt, clock.Now(), "generated at");
    }

    public static void RunAll()
    {
        AdaptiveFloorPlanAdjustmentReportCountsEachRecordOnce();
    }
}
