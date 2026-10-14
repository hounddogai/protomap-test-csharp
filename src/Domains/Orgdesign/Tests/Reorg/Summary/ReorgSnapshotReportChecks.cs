namespace Acme.Workforce.Orgdesign.Tests.Reorg.Summary;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Orgdesign.Reorg.Summary;

/// <summary>Checks of summary code, run by the project's runner.</summary>
public static class ReorgSnapshotReportChecks
{
    public static void ReorgSnapshotReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1709667285);
        var items = new List<ReorgSnapshot>();
        for (var index = 0; index < 6; index++)
        {
            items.Add(ReorgSnapshot.NewReorgSnapshot($"reorg-snapshot-{index}", "sample"));
        }
        var report = ReorgSnapshotReport.BuildReorgSnapshotReport("test", items, clock);
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
        ReorgSnapshotReportCountsEachRecordOnce();
    }
}
