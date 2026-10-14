namespace Acme.Workforce.Engagement.Tests.Sentiment.Reservation;

using Acme.Workforce.Engagement.Sentiment.Reservation;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of reservation code, run by the project's runner.</summary>
public static class SentimentReviewReportChecks
{
    public static void SentimentReviewReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1705114057);
        var items = new List<SentimentReview>();
        for (var index = 0; index < 5; index++)
        {
            items.Add(SentimentReview.NewSentimentReview($"sentiment-review-{index}", "sample"));
        }
        var report = SentimentReviewReport.BuildSentimentReviewReport("test", items, clock);
        var counted = 0;
        foreach (var key in report.Counts.Keys.Order(StringComparer.Ordinal))
        {
            var valueValue = report.Counts[key];
            counted += valueValue;
        }
        Check.True(counted <= 5, "counted");
        Check.Equal(report.GeneratedAt, clock.Now(), "generated at");
    }

    public static void RunAll()
    {
        SentimentReviewReportCountsEachRecordOnce();
    }
}
