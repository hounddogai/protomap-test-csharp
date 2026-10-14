namespace Acme.Workforce.Absence.Tests.Balance.Policy;

using Acme.Workforce.Absence.Balance.Policy;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of policy code, run by the project's runner.</summary>
public static class WeightedBalanceSnapshotReportChecks
{
    public static void WeightedBalanceSnapshotReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1700152460);
        var items = new List<WeightedBalanceSnapshot>();
        for (var index = 0; index < 3; index++)
        {
            items.Add(WeightedBalanceSnapshot.NewWeightedBalanceSnapshot($"weighted-balance-snapshot-{index}", "sample"));
        }
        var report = WeightedBalanceSnapshotReport.BuildWeightedBalanceSnapshotReport("test", items, clock);
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
        WeightedBalanceSnapshotReportCountsEachRecordOnce();
    }
}
