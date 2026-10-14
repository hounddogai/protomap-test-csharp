namespace Acme.Workforce.Compensation.Tests.Adjustment.Request;

using Acme.Workforce.Compensation.Adjustment.Request;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of request code, run by the project's runner.</summary>
public static class BatchedAdjustmentRequestReportChecks
{
    public static void BatchedAdjustmentRequestReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1705592021);
        var items = new List<BatchedAdjustmentRequest>();
        for (var index = 0; index < 3; index++)
        {
            items.Add(BatchedAdjustmentRequest.NewBatchedAdjustmentRequest($"batched-adjustment-request-{index}", "sample"));
        }
        var report = BatchedAdjustmentRequestReport.BuildBatchedAdjustmentRequestReport("test", items, clock);
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
        BatchedAdjustmentRequestReportCountsEachRecordOnce();
    }
}
