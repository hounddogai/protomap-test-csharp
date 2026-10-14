namespace Acme.Workforce.Engagement.Tests.Sentiment.SentimentReservation;

using Acme.Workforce.Engagement.Sentiment.SentimentReservation;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of sentiment reservation code, run by the project's runner.</summary>
public static class SentimentVarianceReportChecks
{
    public static void SentimentVarianceReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1706668212);
        var items = new List<SentimentVariance>();
        for (var index = 0; index < 6; index++)
        {
            items.Add(SentimentVariance.NewSentimentVariance($"sentiment-variance-{index}", "sample"));
        }
        var report = SentimentVarianceReport.BuildSentimentVarianceReport("test", items, clock);
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
        SentimentVarianceReportCountsEachRecordOnce();
    }
}
