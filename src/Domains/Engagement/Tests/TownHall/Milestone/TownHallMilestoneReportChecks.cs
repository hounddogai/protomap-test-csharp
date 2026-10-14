namespace Acme.Workforce.Engagement.Tests.TownHall.Milestone;

using Acme.Workforce.Engagement.TownHall.Milestone;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of milestone code, run by the project's runner.</summary>
public static class TownHallMilestoneReportChecks
{
    public static void TownHallMilestoneReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1706114645);
        var items = new List<TownHallMilestone>();
        for (var index = 0; index < 5; index++)
        {
            items.Add(TownHallMilestone.NewTownHallMilestone($"town-hall-milestone-{index}", "sample"));
        }
        var report = TownHallMilestoneReport.BuildTownHallMilestoneReport("test", items, clock);
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
        TownHallMilestoneReportCountsEachRecordOnce();
    }
}
