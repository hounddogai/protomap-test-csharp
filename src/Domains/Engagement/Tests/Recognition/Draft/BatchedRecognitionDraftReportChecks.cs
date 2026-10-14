namespace Acme.Workforce.Engagement.Tests.Recognition.Draft;

using Acme.Workforce.Engagement.Recognition.Draft;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of draft code, run by the project's runner.</summary>
public static class BatchedRecognitionDraftReportChecks
{
    public static void BatchedRecognitionDraftReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1700009881);
        var items = new List<BatchedRecognitionDraft>();
        for (var index = 0; index < 4; index++)
        {
            items.Add(BatchedRecognitionDraft.NewBatchedRecognitionDraft($"batched-recognition-draft-{index}", "sample"));
        }
        var report = BatchedRecognitionDraftReport.BuildBatchedRecognitionDraftReport("test", items, clock);
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
        BatchedRecognitionDraftReportCountsEachRecordOnce();
    }
}
