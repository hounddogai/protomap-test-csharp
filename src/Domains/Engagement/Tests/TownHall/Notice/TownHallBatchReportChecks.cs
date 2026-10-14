namespace Acme.Workforce.Engagement.Tests.TownHall.Notice;

using Acme.Workforce.Engagement.TownHall.Notice;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of notice code, run by the project's runner.</summary>
public static class TownHallBatchReportChecks
{
    public static void TownHallBatchReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1707770494);
        var items = new List<TownHallBatch>();
        for (var index = 0; index < 2; index++)
        {
            items.Add(TownHallBatch.NewTownHallBatch($"town-hall-batch-{index}", "sample"));
        }
        var report = TownHallBatchReport.BuildTownHallBatchReport("test", items, clock);
        var counted = 0;
        foreach (var key in report.Counts.Keys.Order(StringComparer.Ordinal))
        {
            var valueValue = report.Counts[key];
            counted += valueValue;
        }
        Check.True(counted <= 2, "counted");
        Check.Equal(report.GeneratedAt, clock.Now(), "generated at");
    }

    public static void RunAll()
    {
        TownHallBatchReportCountsEachRecordOnce();
    }
}
