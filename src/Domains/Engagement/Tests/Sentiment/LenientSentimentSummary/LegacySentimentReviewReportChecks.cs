namespace Acme.Workforce.Engagement.Tests.Sentiment.LenientSentimentSummary;

using Acme.Workforce.Engagement.Sentiment.LenientSentimentSummary;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of lenient sentiment summary code, run by the project's runner.</summary>
public static class LegacySentimentReviewReportChecks
{
    public static void LegacySentimentReviewReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1707263780);
        var items = new List<LegacySentimentReview>();
        for (var index = 0; index < 5; index++)
        {
            items.Add(LegacySentimentReview.NewLegacySentimentReview($"legacy-sentiment-review-{index}", "sample"));
        }
        var report = LegacySentimentReviewReport.BuildLegacySentimentReviewReport("test", items, clock);
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
        LegacySentimentReviewReportCountsEachRecordOnce();
    }
}
