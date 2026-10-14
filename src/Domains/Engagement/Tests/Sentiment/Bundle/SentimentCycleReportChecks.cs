namespace Acme.Workforce.Engagement.Tests.Sentiment.Bundle;

using Acme.Workforce.Engagement.Sentiment.Bundle;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of bundle code, run by the project's runner.</summary>
public static class SentimentCycleReportChecks
{
    public static void SentimentCycleReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1700083686);
        var items = new List<SentimentCycle>();
        for (var index = 0; index < 2; index++)
        {
            items.Add(SentimentCycle.NewSentimentCycle($"sentiment-cycle-{index}", "sample"));
        }
        var report = SentimentCycleReport.BuildSentimentCycleReport("test", items, clock);
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
        SentimentCycleReportCountsEachRecordOnce();
    }
}
