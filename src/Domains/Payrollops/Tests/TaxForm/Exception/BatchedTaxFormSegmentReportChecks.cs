namespace Acme.Workforce.Payrollops.Tests.TaxForm.Exception;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Payrollops.TaxForm.Exception;

/// <summary>Checks of exception code, run by the project's runner.</summary>
public static class BatchedTaxFormSegmentReportChecks
{
    public static void BatchedTaxFormSegmentReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1703027669);
        var items = new List<BatchedTaxFormSegment>();
        for (var index = 0; index < 5; index++)
        {
            items.Add(BatchedTaxFormSegment.NewBatchedTaxFormSegment($"batched-tax-form-segment-{index}", "sample"));
        }
        var report = BatchedTaxFormSegmentReport.BuildBatchedTaxFormSegmentReport("test", items, clock);
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
        BatchedTaxFormSegmentReportCountsEachRecordOnce();
    }
}
