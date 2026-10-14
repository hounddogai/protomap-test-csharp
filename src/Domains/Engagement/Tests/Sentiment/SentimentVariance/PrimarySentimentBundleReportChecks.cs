namespace Acme.Workforce.Engagement.Tests.Sentiment.SentimentVariance;

using Acme.Workforce.Engagement.Sentiment.SentimentVariance;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of sentiment variance code, run by the project's runner.</summary>
public static class PrimarySentimentBundleReportChecks
{
    public static void PrimarySentimentBundleReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1700100926);
        var items = new List<PrimarySentimentBundle>();
        for (var index = 0; index < 6; index++)
        {
            items.Add(PrimarySentimentBundle.NewPrimarySentimentBundle($"primary-sentiment-bundle-{index}", "sample"));
        }
        var report = PrimarySentimentBundleReport.BuildPrimarySentimentBundleReport("test", items, clock);
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
        PrimarySentimentBundleReportCountsEachRecordOnce();
    }
}
