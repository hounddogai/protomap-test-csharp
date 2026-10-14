namespace Acme.Workforce.Engagement.Tests.Feedback.RecordItem;

using Acme.Workforce.Engagement.Feedback.RecordItem;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of record item code, run by the project's runner.</summary>
public static class FeedbackScoreReportChecks
{
    public static void FeedbackScoreReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1704730214);
        var items = new List<FeedbackScore>();
        for (var index = 0; index < 4; index++)
        {
            items.Add(FeedbackScore.NewFeedbackScore($"feedback-score-{index}", "sample"));
        }
        var report = FeedbackScoreReport.BuildFeedbackScoreReport("test", items, clock);
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
        FeedbackScoreReportCountsEachRecordOnce();
    }
}
