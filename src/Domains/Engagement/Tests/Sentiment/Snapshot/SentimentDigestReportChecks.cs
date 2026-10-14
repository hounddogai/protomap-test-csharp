namespace Acme.Workforce.Engagement.Tests.Sentiment.Snapshot;

using Acme.Workforce.Engagement.Sentiment.Snapshot;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of snapshot code, run by the project's runner.</summary>
public static class SentimentDigestReportChecks
{
    public static void SentimentDigestReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1703509588);
        var items = new List<SentimentDigest>();
        for (var index = 0; index < 4; index++)
        {
            items.Add(SentimentDigest.NewSentimentDigest($"sentiment-digest-{index}", "sample"));
        }
        var report = SentimentDigestReport.BuildSentimentDigestReport("test", items, clock);
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
        SentimentDigestReportCountsEachRecordOnce();
    }
}
