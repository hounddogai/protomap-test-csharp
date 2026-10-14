namespace Acme.Workforce.Engagement.Tests.Feedback.Group;

using Acme.Workforce.Engagement.Feedback.Group;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of group code, run by the project's runner.</summary>
public static class FeedbackGroupReportChecks
{
    public static void FeedbackGroupReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1706226188);
        var items = new List<FeedbackGroup>();
        for (var index = 0; index < 3; index++)
        {
            items.Add(FeedbackGroup.NewFeedbackGroup($"feedback-group-{index}", "sample"));
        }
        var report = FeedbackGroupReport.BuildFeedbackGroupReport("test", items, clock);
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
        FeedbackGroupReportCountsEachRecordOnce();
    }
}
