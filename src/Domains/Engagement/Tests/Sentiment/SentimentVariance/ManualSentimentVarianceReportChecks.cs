namespace Acme.Workforce.Engagement.Tests.Sentiment.SentimentVariance;

using Acme.Workforce.Engagement.Sentiment.SentimentVariance;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of sentiment variance code, run by the project's runner.</summary>
public static class ManualSentimentVarianceReportChecks
{
    public static void ManualSentimentVarianceReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1708538460);
        var items = new List<ManualSentimentVariance>();
        for (var index = 0; index < 6; index++)
        {
            items.Add(ManualSentimentVariance.NewManualSentimentVariance($"manual-sentiment-variance-{index}", "sample"));
        }
        var report = ManualSentimentVarianceReport.BuildManualSentimentVarianceReport("test", items, clock);
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
        ManualSentimentVarianceReportCountsEachRecordOnce();
    }
}
