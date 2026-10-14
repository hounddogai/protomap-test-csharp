namespace Acme.Workforce.Compensation.Tests.BonusPool.PoolBatch;

using Acme.Workforce.Compensation.BonusPool.PoolBatch;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of pool batch code, run by the project's runner.</summary>
public static class TieredBonusPoolBatchReportChecks
{
    public static void TieredBonusPoolBatchReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1705709928);
        var items = new List<TieredBonusPoolBatch>();
        for (var index = 0; index < 2; index++)
        {
            items.Add(TieredBonusPoolBatch.NewTieredBonusPoolBatch($"tiered-bonus-pool-batch-{index}", "sample"));
        }
        var report = TieredBonusPoolBatchReport.BuildTieredBonusPoolBatchReport("test", items, clock);
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
        TieredBonusPoolBatchReportCountsEachRecordOnce();
    }
}
