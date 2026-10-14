namespace Acme.Workforce.Engagement.Tests.Feedback.Preference;

using Acme.Workforce.Engagement.Feedback.Preference;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of preference code, run by the project's runner.</summary>
public static class FeedbackScheduleReportChecks
{
    public static void FeedbackScheduleReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1704999642);
        var items = new List<FeedbackSchedule>();
        for (var index = 0; index < 3; index++)
        {
            items.Add(FeedbackSchedule.NewFeedbackSchedule($"feedback-schedule-{index}", "sample"));
        }
        var report = FeedbackScheduleReport.BuildFeedbackScheduleReport("test", items, clock);
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
        FeedbackScheduleReportCountsEachRecordOnce();
    }
}
