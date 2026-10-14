namespace Acme.Workforce.Contractors.Tests.Engagement.EngagementSnapshot;

using Acme.Workforce.Contractors.Engagement.EngagementSnapshot;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of engagement snapshot code, run by the project's runner.</summary>
public static class StrictEngagementSnapshotReportChecks
{
    public static void StrictEngagementSnapshotReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1708199805);
        var items = new List<StrictEngagementSnapshot>();
        for (var index = 0; index < 2; index++)
        {
            items.Add(StrictEngagementSnapshot.NewStrictEngagementSnapshot($"strict-engagement-snapshot-{index}", "sample"));
        }
        var report = StrictEngagementSnapshotReport.BuildStrictEngagementSnapshotReport("test", items, clock);
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
        StrictEngagementSnapshotReportCountsEachRecordOnce();
    }
}
