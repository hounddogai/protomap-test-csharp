namespace Acme.Workforce.Engagement.Tests.Sentiment.Proposal;

using Acme.Workforce.Engagement.Sentiment.Proposal;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of proposal code, run by the project's runner.</summary>
public static class SentimentThresholdReportChecks
{
    public static void SentimentThresholdReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1709270957);
        var items = new List<SentimentThreshold>();
        for (var index = 0; index < 6; index++)
        {
            items.Add(SentimentThreshold.NewSentimentThreshold($"sentiment-threshold-{index}", "sample"));
        }
        var report = SentimentThresholdReport.BuildSentimentThresholdReport("test", items, clock);
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
        SentimentThresholdReportCountsEachRecordOnce();
    }
}
