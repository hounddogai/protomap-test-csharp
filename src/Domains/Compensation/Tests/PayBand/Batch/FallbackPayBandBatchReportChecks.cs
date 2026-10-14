namespace Acme.Workforce.Compensation.Tests.PayBand.Batch;

using Acme.Workforce.Compensation.PayBand.Batch;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of batch code, run by the project's runner.</summary>
public static class FallbackPayBandBatchReportChecks
{
    public static void FallbackPayBandBatchReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1708398362);
        var items = new List<FallbackPayBandBatch>();
        for (var index = 0; index < 5; index++)
        {
            items.Add(FallbackPayBandBatch.NewFallbackPayBandBatch($"fallback-pay-band-batch-{index}", "sample"));
        }
        var report = FallbackPayBandBatchReport.BuildFallbackPayBandBatchReport("test", items, clock);
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
        FallbackPayBandBatchReportCountsEachRecordOnce();
    }
}
