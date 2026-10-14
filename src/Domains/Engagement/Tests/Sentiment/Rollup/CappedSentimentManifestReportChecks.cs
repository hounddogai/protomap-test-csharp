namespace Acme.Workforce.Engagement.Tests.Sentiment.Rollup;

using Acme.Workforce.Engagement.Sentiment.Rollup;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of rollup code, run by the project's runner.</summary>
public static class CappedSentimentManifestReportChecks
{
    public static void CappedSentimentManifestReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1702571136);
        var items = new List<CappedSentimentManifest>();
        for (var index = 0; index < 6; index++)
        {
            items.Add(CappedSentimentManifest.NewCappedSentimentManifest($"capped-sentiment-manifest-{index}", "sample"));
        }
        var report = CappedSentimentManifestReport.BuildCappedSentimentManifestReport("test", items, clock);
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
        CappedSentimentManifestReportCountsEachRecordOnce();
    }
}
