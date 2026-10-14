namespace Acme.Workforce.Engagement.Tests.Feedback.Mapping;

using Acme.Workforce.Engagement.Feedback.Mapping;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of mapping code, run by the project's runner.</summary>
public static class PendingFeedbackMilestoneReportChecks
{
    public static void PendingFeedbackMilestoneReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1708487247);
        var items = new List<PendingFeedbackMilestone>();
        for (var index = 0; index < 6; index++)
        {
            items.Add(PendingFeedbackMilestone.NewPendingFeedbackMilestone($"pending-feedback-milestone-{index}", "sample"));
        }
        var report = PendingFeedbackMilestoneReport.BuildPendingFeedbackMilestoneReport("test", items, clock);
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
        PendingFeedbackMilestoneReportCountsEachRecordOnce();
    }
}
