namespace Acme.Workforce.Absence.Tests.Balance.RecordItem;

using Acme.Workforce.Absence.Balance.RecordItem;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of record item code, run by the project's runner.</summary>
public static class BatchedBalanceMappingReportChecks
{
    public static void BatchedBalanceMappingReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1705422080);
        var items = new List<BatchedBalanceMapping>();
        for (var index = 0; index < 3; index++)
        {
            items.Add(BatchedBalanceMapping.NewBatchedBalanceMapping($"batched-balance-mapping-{index}", "sample"));
        }
        var report = BatchedBalanceMappingReport.BuildBatchedBalanceMappingReport("test", items, clock);
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
        BatchedBalanceMappingReportCountsEachRecordOnce();
    }
}
