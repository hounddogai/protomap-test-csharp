namespace Acme.Workforce.Engagement.Tests.Feedback.Checkpoint;

using Acme.Workforce.Engagement.Feedback.Checkpoint;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of checkpoint code, run by the project's runner.</summary>
public static class SeasonalFeedbackMilestoneReportChecks
{
    public static void SeasonalFeedbackMilestoneReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1706735571);
        var items = new List<SeasonalFeedbackMilestone>();
        for (var index = 0; index < 5; index++)
        {
            items.Add(SeasonalFeedbackMilestone.NewSeasonalFeedbackMilestone($"seasonal-feedback-milestone-{index}", "sample"));
        }
        var report = SeasonalFeedbackMilestoneReport.BuildSeasonalFeedbackMilestoneReport("test", items, clock);
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
        SeasonalFeedbackMilestoneReportCountsEachRecordOnce();
    }
}
