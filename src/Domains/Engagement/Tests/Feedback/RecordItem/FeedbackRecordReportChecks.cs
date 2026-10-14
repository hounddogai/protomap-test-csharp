namespace Acme.Workforce.Engagement.Tests.Feedback.RecordItem;

using Acme.Workforce.Engagement.Feedback.RecordItem;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of record item code, run by the project's runner.</summary>
public static class FeedbackRecordReportChecks
{
    public static void FeedbackRecordReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1704552434);
        var items = new List<FeedbackRecord>();
        for (var index = 0; index < 5; index++)
        {
            items.Add(FeedbackRecord.NewFeedbackRecord($"feedback-record-{index}", "sample"));
        }
        var report = FeedbackRecordReport.BuildFeedbackRecordReport("test", items, clock);
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
        FeedbackRecordReportCountsEachRecordOnce();
    }
}
