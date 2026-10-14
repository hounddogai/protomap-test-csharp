namespace Acme.Workforce.Engagement.Tests.Feedback.Reservation;

using Acme.Workforce.Engagement.Feedback.Reservation;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of reservation code, run by the project's runner.</summary>
public static class ScheduledFeedbackRecordReportChecks
{
    public static void ScheduledFeedbackRecordReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1701782011);
        var items = new List<ScheduledFeedbackRecord>();
        for (var index = 0; index < 6; index++)
        {
            items.Add(ScheduledFeedbackRecord.NewScheduledFeedbackRecord($"scheduled-feedback-record-{index}", "sample"));
        }
        var report = ScheduledFeedbackRecordReport.BuildScheduledFeedbackRecordReport("test", items, clock);
        var counted = 0;
        foreach (var key in report.Counts.Keys.Order(StringComparer.Ordinal))
        {
            var valueValue = report.Counts[key];
            counted += valueValue;
        }
        Check.True(counted <= 6, "counted");
        Check.Equal(report.GeneratedAt, clock.Now(), "generated at");
    }

    public static void RunAll()
    {
        ScheduledFeedbackRecordReportCountsEachRecordOnce();
    }
}
