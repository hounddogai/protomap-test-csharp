namespace Acme.Workforce.Engagement.Tests.Feedback.FeedbackScore;

using Acme.Workforce.Engagement.Feedback.FeedbackScore;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of feedback score code, run by the project's runner.</summary>
public static class FeedbackThresholdReportChecks
{
    public static void FeedbackThresholdReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1703897223);
        var items = new List<FeedbackThreshold>();
        for (var index = 0; index < 3; index++)
        {
            items.Add(FeedbackThreshold.NewFeedbackThreshold($"feedback-threshold-{index}", "sample"));
        }
        var report = FeedbackThresholdReport.BuildFeedbackThresholdReport("test", items, clock);
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
        FeedbackThresholdReportCountsEachRecordOnce();
    }
}
