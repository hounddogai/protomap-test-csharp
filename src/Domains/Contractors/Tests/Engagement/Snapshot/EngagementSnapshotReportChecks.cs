namespace Acme.Workforce.Contractors.Tests.Engagement.Snapshot;

using Acme.Workforce.Contractors.Engagement.Snapshot;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of snapshot code, run by the project's runner.</summary>
public static class EngagementSnapshotReportChecks
{
    public static void EngagementSnapshotReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1705683036);
        var items = new List<EngagementSnapshot>();
        for (var index = 0; index < 4; index++)
        {
            items.Add(EngagementSnapshot.NewEngagementSnapshot($"engagement-snapshot-{index}", "sample"));
        }
        var report = EngagementSnapshotReport.BuildEngagementSnapshotReport("test", items, clock);
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
        EngagementSnapshotReportCountsEachRecordOnce();
    }
}
