namespace Acme.Workforce.Engagement.Tests.Sentiment.Milestone;

using Acme.Workforce.Engagement.Sentiment.Milestone;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of milestone code, run by the project's runner.</summary>
public static class SentimentBatchReportChecks
{
    public static void SentimentBatchReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1703884789);
        var items = new List<SentimentBatch>();
        for (var index = 0; index < 6; index++)
        {
            items.Add(SentimentBatch.NewSentimentBatch($"sentiment-batch-{index}", "sample"));
        }
        var report = SentimentBatchReport.BuildSentimentBatchReport("test", items, clock);
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
        SentimentBatchReportCountsEachRecordOnce();
    }
}
