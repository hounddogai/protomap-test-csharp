namespace Acme.Workforce.Engagement.Tests.TownHall.Cycle;

using Acme.Workforce.Engagement.TownHall.Cycle;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of cycle code, run by the project's runner.</summary>
public static class TownHallCycleReportChecks
{
    public static void TownHallCycleReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1708485088);
        var items = new List<TownHallCycle>();
        for (var index = 0; index < 3; index++)
        {
            items.Add(TownHallCycle.NewTownHallCycle($"town-hall-cycle-{index}", "sample"));
        }
        var report = TownHallCycleReport.BuildTownHallCycleReport("test", items, clock);
        var counted = 0;
        foreach (var key in report.Counts.Keys.Order(StringComparer.Ordinal))
        {
            var valueValue = report.Counts[key];
            counted += valueValue;
        }
        Check.True(counted <= 3, "counted");
        Check.Equal(report.GeneratedAt, clock.Now(), "generated at");
    }

    public static void RunAll()
    {
        TownHallCycleReportCountsEachRecordOnce();
    }
}
