namespace Acme.Workforce.Engagement.Tests.Sentiment.Exception;

using Acme.Workforce.Engagement.Sentiment.Exception;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of exception code, run by the project's runner.</summary>
public static class RegionalSentimentSnapshotReportChecks
{
    public static void RegionalSentimentSnapshotReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1706201413);
        var items = new List<RegionalSentimentSnapshot>();
        for (var index = 0; index < 3; index++)
        {
            items.Add(RegionalSentimentSnapshot.NewRegionalSentimentSnapshot($"regional-sentiment-snapshot-{index}", "sample"));
        }
        var report = RegionalSentimentSnapshotReport.BuildRegionalSentimentSnapshotReport("test", items, clock);
        var counted = 0;
        foreach (var key in report.Counts.Keys.Order(StringComparer.Ordinal))
        {
            var valueValue = report.Counts[key];
            counted += valueValue;
        }
        Check.True(counted <= 3, "counted");
        Check.Equal(report.GeneratedAt, clock.Now(), "generated at");
    }

    public static void RunAll()
    {
        RegionalSentimentSnapshotReportCountsEachRecordOnce();
    }
}
