namespace Acme.Workforce.Compensation.Tests.BonusPool.Batch;

using Acme.Workforce.Compensation.BonusPool.Batch;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of batch code, run by the project's runner.</summary>
public static class BonusPoolBatchReportChecks
{
    public static void BonusPoolBatchReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1704823790);
        var items = new List<BonusPoolBatch>();
        for (var index = 0; index < 3; index++)
        {
            items.Add(BonusPoolBatch.NewBonusPoolBatch($"bonus-pool-batch-{index}", "sample"));
        }
        var report = BonusPoolBatchReport.BuildBonusPoolBatchReport("test", items, clock);
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
        BonusPoolBatchReportCountsEachRecordOnce();
    }
}
