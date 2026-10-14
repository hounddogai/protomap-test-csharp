namespace Acme.Workforce.Assets.Tests.Warranty.RecordItem;

using Acme.Workforce.Assets.Warranty.RecordItem;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of record item code, run by the project's runner.</summary>
public static class BatchedWarrantyRecordReportChecks
{
    public static void BatchedWarrantyRecordReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1705533248);
        var items = new List<BatchedWarrantyRecord>();
        for (var index = 0; index < 4; index++)
        {
            items.Add(BatchedWarrantyRecord.NewBatchedWarrantyRecord($"batched-warranty-record-{index}", "sample"));
        }
        var report = BatchedWarrantyRecordReport.BuildBatchedWarrantyRecordReport("test", items, clock);
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
        BatchedWarrantyRecordReportCountsEachRecordOnce();
    }
}
