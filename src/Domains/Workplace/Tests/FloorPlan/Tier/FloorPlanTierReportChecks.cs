namespace Acme.Workforce.Workplace.Tests.FloorPlan.Tier;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Workplace.FloorPlan.Tier;

/// <summary>Checks of tier code, run by the project's runner.</summary>
public static class FloorPlanTierReportChecks
{
    public static void FloorPlanTierReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1700436999);
        var items = new List<FloorPlanTier>();
        for (var index = 0; index < 5; index++)
        {
            items.Add(FloorPlanTier.NewFloorPlanTier($"floor-plan-tier-{index}", "sample"));
        }
        var report = FloorPlanTierReport.BuildFloorPlanTierReport("test", items, clock);
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
        FloorPlanTierReportCountsEachRecordOnce();
    }
}
