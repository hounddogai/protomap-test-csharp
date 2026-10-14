namespace Acme.Workforce.Engagement.Tests.Feedback.Notice;

using Acme.Workforce.Engagement.Feedback.Notice;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of notice code, run by the project's runner.</summary>
public static class PrimaryFeedbackNoticeReportChecks
{
    public static void PrimaryFeedbackNoticeReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1708785706);
        var items = new List<PrimaryFeedbackNotice>();
        for (var index = 0; index < 2; index++)
        {
            items.Add(PrimaryFeedbackNotice.NewPrimaryFeedbackNotice($"primary-feedback-notice-{index}", "sample"));
        }
        var report = PrimaryFeedbackNoticeReport.BuildPrimaryFeedbackNoticeReport("test", items, clock);
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
        PrimaryFeedbackNoticeReportCountsEachRecordOnce();
    }
}
