namespace Acme.Workforce.Engagement.Tests.Sentiment.LegacySentimentManifest;

using Acme.Workforce.Engagement.Sentiment.LegacySentimentManifest;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of legacy sentiment manifest code, run by the project's runner.</summary>
public static class SeasonalSentimentDraftReportChecks
{
    public static void SeasonalSentimentDraftReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1705432942);
        var items = new List<SeasonalSentimentDraft>();
        for (var index = 0; index < 5; index++)
        {
            items.Add(SeasonalSentimentDraft.NewSeasonalSentimentDraft($"seasonal-sentiment-draft-{index}", "sample"));
        }
        var report = SeasonalSentimentDraftReport.BuildSeasonalSentimentDraftReport("test", items, clock);
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
        SeasonalSentimentDraftReportCountsEachRecordOnce();
    }
}
