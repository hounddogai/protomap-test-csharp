namespace Acme.Workforce.Engagement.Tests.Sentiment.Threshold;

using Acme.Workforce.Engagement.Sentiment.Threshold;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of threshold code, run by the project's runner.</summary>
public static class FallbackSentimentScoreReportChecks
{
    public static void FallbackSentimentScoreReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1709029305);
        var items = new List<FallbackSentimentScore>();
        for (var index = 0; index < 3; index++)
        {
            items.Add(FallbackSentimentScore.NewFallbackSentimentScore($"fallback-sentiment-score-{index}", "sample"));
        }
        var report = FallbackSentimentScoreReport.BuildFallbackSentimentScoreReport("test", items, clock);
        var counted = 0;
        foreach (var key in report.Counts.Keys.Order(StringComparer.Ordinal))
        {
            var valueValue = report.Counts[key];
            counted += valueValue;
        }
        Check.True(counted <= 3, "counted");
        Check.Equal(report.GeneratedAt, clock.Now(), "generated at");
    }

    public static void RunAll()
    {
        FallbackSentimentScoreReportCountsEachRecordOnce();
    }
}
