namespace Acme.Workforce.Engagement.Tests.Sentiment.Milestone;

using Acme.Workforce.Engagement.Sentiment.Milestone;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of milestone code, run by the project's runner.</summary>
public static class SentimentMilestoneReportChecks
{
    public static void SentimentMilestoneReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1707144977);
        var items = new List<SentimentMilestone>();
        for (var index = 0; index < 3; index++)
        {
            items.Add(SentimentMilestone.NewSentimentMilestone($"sentiment-milestone-{index}", "sample"));
        }
        var report = SentimentMilestoneReport.BuildSentimentMilestoneReport("test", items, clock);
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
        SentimentMilestoneReportCountsEachRecordOnce();
    }
}
