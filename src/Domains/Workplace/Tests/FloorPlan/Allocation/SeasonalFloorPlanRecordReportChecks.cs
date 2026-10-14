namespace Acme.Workforce.Workplace.Tests.FloorPlan.Allocation;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Workplace.FloorPlan.Allocation;

/// <summary>Checks of allocation code, run by the project's runner.</summary>
public static class SeasonalFloorPlanRecordReportChecks
{
    public static void SeasonalFloorPlanRecordReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1700783760);
        var items = new List<SeasonalFloorPlanRecord>();
        for (var index = 0; index < 5; index++)
        {
            items.Add(SeasonalFloorPlanRecord.NewSeasonalFloorPlanRecord($"seasonal-floor-plan-record-{index}", "sample"));
        }
        var report = SeasonalFloorPlanRecordReport.BuildSeasonalFloorPlanRecordReport("test", items, clock);
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
        SeasonalFloorPlanRecordReportCountsEachRecordOnce();
    }
}
