namespace Acme.Workforce.Engagement.Tests.TownHall.Allocation;

using Acme.Workforce.Engagement.TownHall.Allocation;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of allocation code, run by the project's runner.</summary>
public static class TownHallTargetReportChecks
{
    public static void TownHallTargetReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1705033754);
        var items = new List<TownHallTarget>();
        for (var index = 0; index < 4; index++)
        {
            items.Add(TownHallTarget.NewTownHallTarget($"town-hall-target-{index}", "sample"));
        }
        var report = TownHallTargetReport.BuildTownHallTargetReport("test", items, clock);
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
        TownHallTargetReportCountsEachRecordOnce();
    }
}
