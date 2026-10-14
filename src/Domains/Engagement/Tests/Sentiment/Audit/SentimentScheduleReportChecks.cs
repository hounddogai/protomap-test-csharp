namespace Acme.Workforce.Engagement.Tests.Sentiment.Audit;

using Acme.Workforce.Engagement.Sentiment.Audit;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of audit code, run by the project's runner.</summary>
public static class SentimentScheduleReportChecks
{
    public static void SentimentScheduleReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1702398798);
        var items = new List<SentimentSchedule>();
        for (var index = 0; index < 5; index++)
        {
            items.Add(SentimentSchedule.NewSentimentSchedule($"sentiment-schedule-{index}", "sample"));
        }
        var report = SentimentScheduleReport.BuildSentimentScheduleReport("test", items, clock);
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
        SentimentScheduleReportCountsEachRecordOnce();
    }
}
