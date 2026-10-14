namespace Acme.Workforce.Engagement.Tests.Feedback.Quota;

using Acme.Workforce.Engagement.Feedback.Quota;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of quota code, run by the project's runner.</summary>
public static class FeedbackNoticeReportChecks
{
    public static void FeedbackNoticeReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1709805420);
        var items = new List<FeedbackNotice>();
        for (var index = 0; index < 2; index++)
        {
            items.Add(FeedbackNotice.NewFeedbackNotice($"feedback-notice-{index}", "sample"));
        }
        var report = FeedbackNoticeReport.BuildFeedbackNoticeReport("test", items, clock);
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
        FeedbackNoticeReportCountsEachRecordOnce();
    }
}
