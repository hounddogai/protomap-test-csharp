namespace Acme.Workforce.Payrollops.Tests.PayRun.RunVariance;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Payrollops.PayRun.RunVariance;

/// <summary>Checks of run variance code, run by the project's runner.</summary>
public static class BatchedPayRunVarianceReportChecks
{
    public static void BatchedPayRunVarianceReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1701893119);
        var items = new List<BatchedPayRunVariance>();
        for (var index = 0; index < 6; index++)
        {
            items.Add(BatchedPayRunVariance.NewBatchedPayRunVariance($"batched-pay-run-variance-{index}", "sample"));
        }
        var report = BatchedPayRunVarianceReport.BuildBatchedPayRunVarianceReport("test", items, clock);
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
        BatchedPayRunVarianceReportCountsEachRecordOnce();
    }
}
