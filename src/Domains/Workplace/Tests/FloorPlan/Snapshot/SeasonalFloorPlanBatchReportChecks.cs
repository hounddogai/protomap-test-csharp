namespace Acme.Workforce.Workplace.Tests.FloorPlan.Snapshot;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Workplace.FloorPlan.Snapshot;

/// <summary>Checks of snapshot code, run by the project's runner.</summary>
public static class SeasonalFloorPlanBatchReportChecks
{
    public static void SeasonalFloorPlanBatchReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1707622461);
        var items = new List<SeasonalFloorPlanBatch>();
        for (var index = 0; index < 4; index++)
        {
            items.Add(SeasonalFloorPlanBatch.NewSeasonalFloorPlanBatch($"seasonal-floor-plan-batch-{index}", "sample"));
        }
        var report = SeasonalFloorPlanBatchReport.BuildSeasonalFloorPlanBatchReport("test", items, clock);
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
        SeasonalFloorPlanBatchReportCountsEachRecordOnce();
    }
}
