namespace Acme.Workforce.Compensation.Tests.BonusPool.Window;

using Acme.Workforce.Compensation.BonusPool.Window;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of window code, run by the project's runner.</summary>
public static class BonusPoolSnapshotReportChecks
{
    public static void BonusPoolSnapshotReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1708534899);
        var items = new List<BonusPoolSnapshot>();
        for (var index = 0; index < 5; index++)
        {
            items.Add(BonusPoolSnapshot.NewBonusPoolSnapshot($"bonus-pool-snapshot-{index}", "sample"));
        }
        var report = BonusPoolSnapshotReport.BuildBonusPoolSnapshotReport("test", items, clock);
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
        BonusPoolSnapshotReportCountsEachRecordOnce();
    }
}
