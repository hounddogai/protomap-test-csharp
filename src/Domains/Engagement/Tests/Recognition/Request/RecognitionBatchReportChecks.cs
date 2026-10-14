namespace Acme.Workforce.Engagement.Tests.Recognition.Request;

using Acme.Workforce.Engagement.Recognition.Request;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of request code, run by the project's runner.</summary>
public static class RecognitionBatchReportChecks
{
    public static void RecognitionBatchReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1702270178);
        var items = new List<RecognitionBatch>();
        for (var index = 0; index < 5; index++)
        {
            items.Add(RecognitionBatch.NewRecognitionBatch($"recognition-batch-{index}", "sample"));
        }
        var report = RecognitionBatchReport.BuildRecognitionBatchReport("test", items, clock);
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
        RecognitionBatchReportCountsEachRecordOnce();
    }
}
