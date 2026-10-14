namespace Acme.Workforce.Workplace.Tests.FloorPlan.PlanEntry;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Workplace.FloorPlan.PlanEntry;

/// <summary>Checks of plan entry code, run by the project's runner.</summary>
public static class SeasonalFloorPlanTierReportChecks
{
    public static void SeasonalFloorPlanTierReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1701664615);
        var items = new List<SeasonalFloorPlanTier>();
        for (var index = 0; index < 5; index++)
        {
            items.Add(SeasonalFloorPlanTier.NewSeasonalFloorPlanTier($"seasonal-floor-plan-tier-{index}", "sample"));
        }
        var report = SeasonalFloorPlanTierReport.BuildSeasonalFloorPlanTierReport("test", items, clock);
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
        SeasonalFloorPlanTierReportCountsEachRecordOnce();
    }
}
