namespace Acme.Workforce.Engagement.Tests.Sentiment.LenientSentimentSummary;

using Acme.Workforce.Engagement.Sentiment.LenientSentimentSummary;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of lenient sentiment summary code, run by the project's runner.</summary>
public static class SentimentForecastReportChecks
{
    public static void SentimentForecastReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1709404121);
        var items = new List<SentimentForecast>();
        for (var index = 0; index < 6; index++)
        {
            items.Add(SentimentForecast.NewSentimentForecast($"sentiment-forecast-{index}", "sample"));
        }
        var report = SentimentForecastReport.BuildSentimentForecastReport("test", items, clock);
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
        SentimentForecastReportCountsEachRecordOnce();
    }
}
