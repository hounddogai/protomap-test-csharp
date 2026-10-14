namespace Acme.Workforce.Engagement.Tests.TownHall.Exception;

using Acme.Workforce.Engagement.TownHall.Exception;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of exception code, run by the project's runner.</summary>
public static class TownHallCheckpointReportChecks
{
    public static void TownHallCheckpointReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1706648403);
        var items = new List<TownHallCheckpoint>();
        for (var index = 0; index < 5; index++)
        {
            items.Add(TownHallCheckpoint.NewTownHallCheckpoint($"town-hall-checkpoint-{index}", "sample"));
        }
        var report = TownHallCheckpointReport.BuildTownHallCheckpointReport("test", items, clock);
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
        TownHallCheckpointReportCountsEachRecordOnce();
    }
}
