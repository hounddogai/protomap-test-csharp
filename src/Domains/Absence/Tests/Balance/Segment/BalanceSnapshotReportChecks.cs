namespace Acme.Workforce.Absence.Tests.Balance.Segment;

using Acme.Workforce.Absence.Balance.Segment;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of segment code, run by the project's runner.</summary>
public static class BalanceSnapshotReportChecks
{
    public static void BalanceSnapshotReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1708710268);
        var items = new List<BalanceSnapshot>();
        for (var index = 0; index < 3; index++)
        {
            items.Add(BalanceSnapshot.NewBalanceSnapshot($"balance-snapshot-{index}", "sample"));
        }
        var report = BalanceSnapshotReport.BuildBalanceSnapshotReport("test", items, clock);
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
        BalanceSnapshotReportCountsEachRecordOnce();
    }
}
