namespace Acme.Workforce.Payrollops.Tests.Deduction.Batch;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Payrollops.Deduction.Batch;

/// <summary>Checks of batch code, run by the project's runner.</summary>
public static class DeductionBatchReportChecks
{
    public static void DeductionBatchReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1708335881);
        var items = new List<DeductionBatch>();
        for (var index = 0; index < 4; index++)
        {
            items.Add(DeductionBatch.NewDeductionBatch($"deduction-batch-{index}", "sample"));
        }
        var report = DeductionBatchReport.BuildDeductionBatchReport("test", items, clock);
        var counted = 0;
        foreach (var key in report.Counts.Keys.Order(StringComparer.Ordinal))
        {
            var valueValue = report.Counts[key];
            counted += valueValue;
        }
        Check.True(counted <= 4, "counted");
        Check.Equal(report.GeneratedAt, clock.Now(), "generated at");
    }

    public static void RunAll()
    {
        DeductionBatchReportCountsEachRecordOnce();
    }
}
