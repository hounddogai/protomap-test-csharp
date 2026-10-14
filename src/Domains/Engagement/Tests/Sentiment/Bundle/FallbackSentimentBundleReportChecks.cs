namespace Acme.Workforce.Engagement.Tests.Sentiment.Bundle;

using Acme.Workforce.Engagement.Sentiment.Bundle;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of bundle code, run by the project's runner.</summary>
public static class FallbackSentimentBundleReportChecks
{
    public static void FallbackSentimentBundleReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1708473279);
        var items = new List<FallbackSentimentBundle>();
        for (var index = 0; index < 5; index++)
        {
            items.Add(FallbackSentimentBundle.NewFallbackSentimentBundle($"fallback-sentiment-bundle-{index}", "sample"));
        }
        var report = FallbackSentimentBundleReport.BuildFallbackSentimentBundleReport("test", items, clock);
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
        FallbackSentimentBundleReportCountsEachRecordOnce();
    }
}
