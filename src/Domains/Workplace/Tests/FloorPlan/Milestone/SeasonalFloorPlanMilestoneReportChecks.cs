namespace Acme.Workforce.Workplace.Tests.FloorPlan.Milestone;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Workplace.FloorPlan.Milestone;

/// <summary>Checks of milestone code, run by the project's runner.</summary>
public static class SeasonalFloorPlanMilestoneReportChecks
{
    public static void SeasonalFloorPlanMilestoneReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1707208337);
        var items = new List<SeasonalFloorPlanMilestone>();
        for (var index = 0; index < 6; index++)
        {
            items.Add(SeasonalFloorPlanMilestone.NewSeasonalFloorPlanMilestone($"seasonal-floor-plan-milestone-{index}", "sample"));
        }
        var report = SeasonalFloorPlanMilestoneReport.BuildSeasonalFloorPlanMilestoneReport("test", items, clock);
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
        SeasonalFloorPlanMilestoneReportCountsEachRecordOnce();
    }
}
