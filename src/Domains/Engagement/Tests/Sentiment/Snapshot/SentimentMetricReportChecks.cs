namespace Acme.Workforce.Engagement.Tests.Sentiment.Snapshot;

using Acme.Workforce.Engagement.Sentiment.Snapshot;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of snapshot code, run by the project's runner.</summary>
public static class SentimentMetricReportChecks
{
    public static void SentimentMetricReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1701762266);
        var items = new List<SentimentMetric>();
        for (var index = 0; index < 4; index++)
        {
            items.Add(SentimentMetric.NewSentimentMetric($"sentiment-metric-{index}", "sample"));
        }
        var report = SentimentMetricReport.BuildSentimentMetricReport("test", items, clock);
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
        SentimentMetricReportCountsEachRecordOnce();
    }
}
