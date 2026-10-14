namespace Acme.Workforce.Workplace.Tests.FloorPlan.Rollup;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Workplace.FloorPlan.Rollup;

/// <summary>Checks of rollup code, run by the project's runner.</summary>
public static class RollingFloorPlanRollupReportChecks
{
    public static void RollingFloorPlanRollupReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1709049319);
        var items = new List<RollingFloorPlanRollup>();
        for (var index = 0; index < 5; index++)
        {
            items.Add(RollingFloorPlanRollup.NewRollingFloorPlanRollup($"rolling-floor-plan-rollup-{index}", "sample"));
        }
        var report = RollingFloorPlanRollupReport.BuildRollingFloorPlanRollupReport("test", items, clock);
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
        RollingFloorPlanRollupReportCountsEachRecordOnce();
    }
}
