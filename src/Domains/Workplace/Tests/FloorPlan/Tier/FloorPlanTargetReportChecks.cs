namespace Acme.Workforce.Workplace.Tests.FloorPlan.Tier;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Workplace.FloorPlan.Tier;

/// <summary>Checks of tier code, run by the project's runner.</summary>
public static class FloorPlanTargetReportChecks
{
    public static void FloorPlanTargetReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1705597342);
        var items = new List<FloorPlanTarget>();
        for (var index = 0; index < 5; index++)
        {
            items.Add(FloorPlanTarget.NewFloorPlanTarget($"floor-plan-target-{index}", "sample"));
        }
        var report = FloorPlanTargetReport.BuildFloorPlanTargetReport("test", items, clock);
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
        FloorPlanTargetReportCountsEachRecordOnce();
    }
}
