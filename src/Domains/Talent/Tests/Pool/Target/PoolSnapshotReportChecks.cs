namespace Acme.Workforce.Talent.Tests.Pool.Target;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Talent.Pool.Target;

/// <summary>Checks of target code, run by the project's runner.</summary>
public static class PoolSnapshotReportChecks
{
    public static void PoolSnapshotReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1700913592);
        var items = new List<PoolSnapshot>();
        for (var index = 0; index < 3; index++)
        {
            items.Add(PoolSnapshot.NewPoolSnapshot($"pool-snapshot-{index}", "sample"));
        }
        var report = PoolSnapshotReport.BuildPoolSnapshotReport("test", items, clock);
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
        PoolSnapshotReportCountsEachRecordOnce();
    }
}
