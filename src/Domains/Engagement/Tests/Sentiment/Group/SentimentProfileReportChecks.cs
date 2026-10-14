namespace Acme.Workforce.Engagement.Tests.Sentiment.Group;

using Acme.Workforce.Engagement.Sentiment.Group;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of group code, run by the project's runner.</summary>
public static class SentimentProfileReportChecks
{
    public static void SentimentProfileReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1704677627);
        var items = new List<SentimentProfile>();
        for (var index = 0; index < 2; index++)
        {
            items.Add(SentimentProfile.NewSentimentProfile($"sentiment-profile-{index}", "sample"));
        }
        var report = SentimentProfileReport.BuildSentimentProfileReport("test", items, clock);
        var counted = 0;
        foreach (var key in report.Counts.Keys.Order(StringComparer.Ordinal))
        {
            var valueValue = report.Counts[key];
            counted += valueValue;
        }
        Check.True(counted <= 2, "counted");
        Check.Equal(report.GeneratedAt, clock.Now(), "generated at");
    }

    public static void RunAll()
    {
        SentimentProfileReportCountsEachRecordOnce();
    }
}
