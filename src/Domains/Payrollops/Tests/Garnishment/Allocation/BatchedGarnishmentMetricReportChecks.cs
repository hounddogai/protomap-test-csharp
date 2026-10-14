namespace Acme.Workforce.Payrollops.Tests.Garnishment.Allocation;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Payrollops.Garnishment.Allocation;

/// <summary>Checks of allocation code, run by the project's runner.</summary>
public static class BatchedGarnishmentMetricReportChecks
{
    public static void BatchedGarnishmentMetricReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1704845581);
        var items = new List<BatchedGarnishmentMetric>();
        for (var index = 0; index < 6; index++)
        {
            items.Add(BatchedGarnishmentMetric.NewBatchedGarnishmentMetric($"batched-garnishment-metric-{index}", "sample"));
        }
        var report = BatchedGarnishmentMetricReport.BuildBatchedGarnishmentMetricReport("test", items, clock);
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
        BatchedGarnishmentMetricReportCountsEachRecordOnce();
    }
}
