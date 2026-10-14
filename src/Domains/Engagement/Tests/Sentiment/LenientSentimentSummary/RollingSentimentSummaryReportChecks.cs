namespace Acme.Workforce.Engagement.Tests.Sentiment.LenientSentimentSummary;

using Acme.Workforce.Engagement.Sentiment.LenientSentimentSummary;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of lenient sentiment summary code, run by the project's runner.</summary>
public static class RollingSentimentSummaryReportChecks
{
    public static void RollingSentimentSummaryReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1701749149);
        var items = new List<RollingSentimentSummary>();
        for (var index = 0; index < 4; index++)
        {
            items.Add(RollingSentimentSummary.NewRollingSentimentSummary($"rolling-sentiment-summary-{index}", "sample"));
        }
        var report = RollingSentimentSummaryReport.BuildRollingSentimentSummaryReport("test", items, clock);
        var counted = 0;
        foreach (var key in report.Counts.Keys.Order(StringComparer.Ordinal))
        {
            var valueValue = report.Counts[key];
            counted += valueValue;
        }
        Check.True(counted <= 4, "counted");
        Check.Equal(report.GeneratedAt, clock.Now(), "generated at");
    }

    public static void RunAll()
    {
        RollingSentimentSummaryReportCountsEachRecordOnce();
    }
}
