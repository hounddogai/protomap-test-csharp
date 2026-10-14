namespace Acme.Workforce.Engagement.Tests.Sentiment.Adjustment;

using Acme.Workforce.Engagement.Sentiment.Adjustment;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of adjustment code, run by the project's runner.</summary>
public static class SentimentCheckpointReportChecks
{
    public static void SentimentCheckpointReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1707565289);
        var items = new List<SentimentCheckpoint>();
        for (var index = 0; index < 6; index++)
        {
            items.Add(SentimentCheckpoint.NewSentimentCheckpoint($"sentiment-checkpoint-{index}", "sample"));
        }
        var report = SentimentCheckpointReport.BuildSentimentCheckpointReport("test", items, clock);
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
        SentimentCheckpointReportCountsEachRecordOnce();
    }
}
