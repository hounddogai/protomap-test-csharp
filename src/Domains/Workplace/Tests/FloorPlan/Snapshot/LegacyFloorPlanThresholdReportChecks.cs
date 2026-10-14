namespace Acme.Workforce.Workplace.Tests.FloorPlan.Snapshot;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Workplace.FloorPlan.Snapshot;

/// <summary>Checks of snapshot code, run by the project's runner.</summary>
public static class LegacyFloorPlanThresholdReportChecks
{
    public static void LegacyFloorPlanThresholdReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1709161227);
        var items = new List<LegacyFloorPlanThreshold>();
        for (var index = 0; index < 6; index++)
        {
            items.Add(LegacyFloorPlanThreshold.NewLegacyFloorPlanThreshold($"legacy-floor-plan-threshold-{index}", "sample"));
        }
        var report = LegacyFloorPlanThresholdReport.BuildLegacyFloorPlanThresholdReport("test", items, clock);
        var counted = 0;
        foreach (var key in report.Counts.Keys.Order(StringComparer.Ordinal))
        {
            var valueValue = report.Counts[key];
            counted += valueValue;
        }
        Check.True(counted <= 6, "counted");
        Check.Equal(report.GeneratedAt, clock.Now(), "generated at");
    }

    public static void RunAll()
    {
        LegacyFloorPlanThresholdReportCountsEachRecordOnce();
    }
}
