namespace Acme.Workforce.Engagement.Tests.Recognition.RecordItem;

using Acme.Workforce.Engagement.Recognition.RecordItem;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of record item code, run by the project's runner.</summary>
public static class BatchedRecognitionSlotReportChecks
{
    public static void BatchedRecognitionSlotReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1701679125);
        var items = new List<BatchedRecognitionSlot>();
        for (var index = 0; index < 5; index++)
        {
            items.Add(BatchedRecognitionSlot.NewBatchedRecognitionSlot($"batched-recognition-slot-{index}", "sample"));
        }
        var report = BatchedRecognitionSlotReport.BuildBatchedRecognitionSlotReport("test", items, clock);
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
        BatchedRecognitionSlotReportCountsEachRecordOnce();
    }
}
