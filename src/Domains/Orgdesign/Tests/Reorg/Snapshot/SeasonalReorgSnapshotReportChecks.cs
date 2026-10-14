namespace Acme.Workforce.Orgdesign.Tests.Reorg.Snapshot;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Orgdesign.Reorg.Snapshot;

/// <summary>Checks of snapshot code, run by the project's runner.</summary>
public static class SeasonalReorgSnapshotReportChecks
{
    public static void SeasonalReorgSnapshotReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1707159140);
        var items = new List<SeasonalReorgSnapshot>();
        for (var index = 0; index < 4; index++)
        {
            items.Add(SeasonalReorgSnapshot.NewSeasonalReorgSnapshot($"seasonal-reorg-snapshot-{index}", "sample"));
        }
        var report = SeasonalReorgSnapshotReport.BuildSeasonalReorgSnapshotReport("test", items, clock);
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
        SeasonalReorgSnapshotReportCountsEachRecordOnce();
    }
}
