namespace Acme.Workforce.Workplace.Tests.Occupancy.Snapshot;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Workplace.Occupancy.Snapshot;

/// <summary>Checks of snapshot code, run by the project's runner.</summary>
public static class ScheduledOccupancySnapshotReportChecks
{
    public static void ScheduledOccupancySnapshotReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1706038288);
        var items = new List<ScheduledOccupancySnapshot>();
        for (var index = 0; index < 6; index++)
        {
            items.Add(ScheduledOccupancySnapshot.NewScheduledOccupancySnapshot($"scheduled-occupancy-snapshot-{index}", "sample"));
        }
        var report = ScheduledOccupancySnapshotReport.BuildScheduledOccupancySnapshotReport("test", items, clock);
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
        ScheduledOccupancySnapshotReportCountsEachRecordOnce();
    }
}
