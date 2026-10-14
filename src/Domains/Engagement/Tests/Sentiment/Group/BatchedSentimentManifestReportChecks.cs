namespace Acme.Workforce.Engagement.Tests.Sentiment.Group;

using Acme.Workforce.Engagement.Sentiment.Group;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of group code, run by the project's runner.</summary>
public static class BatchedSentimentManifestReportChecks
{
    public static void BatchedSentimentManifestReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1707780678);
        var items = new List<BatchedSentimentManifest>();
        for (var index = 0; index < 4; index++)
        {
            items.Add(BatchedSentimentManifest.NewBatchedSentimentManifest($"batched-sentiment-manifest-{index}", "sample"));
        }
        var report = BatchedSentimentManifestReport.BuildBatchedSentimentManifestReport("test", items, clock);
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
        BatchedSentimentManifestReportCountsEachRecordOnce();
    }
}
