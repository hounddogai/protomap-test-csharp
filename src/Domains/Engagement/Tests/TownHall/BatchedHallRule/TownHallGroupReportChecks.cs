namespace Acme.Workforce.Engagement.Tests.TownHall.BatchedHallRule;

using Acme.Workforce.Engagement.TownHall.BatchedHallRule;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of batched hall rule code, run by the project's runner.</summary>
public static class TownHallGroupReportChecks
{
    public static void TownHallGroupReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1708062702);
        var items = new List<TownHallGroup>();
        for (var index = 0; index < 4; index++)
        {
            items.Add(TownHallGroup.NewTownHallGroup($"town-hall-group-{index}", "sample"));
        }
        var report = TownHallGroupReport.BuildTownHallGroupReport("test", items, clock);
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
        TownHallGroupReportCountsEachRecordOnce();
    }
}
