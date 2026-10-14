namespace Acme.Workforce.Compensation.Tests.PayBand.BandThreshold;

using Acme.Workforce.Compensation.PayBand.BandThreshold;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of band threshold code, run by the project's runner.</summary>
public static class BatchedPayBandThresholdReportChecks
{
    public static void BatchedPayBandThresholdReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1700632685);
        var items = new List<BatchedPayBandThreshold>();
        for (var index = 0; index < 3; index++)
        {
            items.Add(BatchedPayBandThreshold.NewBatchedPayBandThreshold($"batched-pay-band-threshold-{index}", "sample"));
        }
        var report = BatchedPayBandThresholdReport.BuildBatchedPayBandThresholdReport("test", items, clock);
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
        BatchedPayBandThresholdReportCountsEachRecordOnce();
    }
}
