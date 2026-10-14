namespace Acme.Workforce.Engagement.Tests.TownHall.Threshold;

using Acme.Workforce.Engagement.TownHall.Threshold;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of threshold code, run by the project's runner.</summary>
public static class LegacyTownHallCheckpointReportChecks
{
    public static void LegacyTownHallCheckpointReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1705529173);
        var items = new List<LegacyTownHallCheckpoint>();
        for (var index = 0; index < 2; index++)
        {
            items.Add(LegacyTownHallCheckpoint.NewLegacyTownHallCheckpoint($"legacy-town-hall-checkpoint-{index}", "sample"));
        }
        var report = LegacyTownHallCheckpointReport.BuildLegacyTownHallCheckpointReport("test", items, clock);
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
        LegacyTownHallCheckpointReportCountsEachRecordOnce();
    }
}
