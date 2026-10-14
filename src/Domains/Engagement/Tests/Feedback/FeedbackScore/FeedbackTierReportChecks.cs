namespace Acme.Workforce.Engagement.Tests.Feedback.FeedbackScore;

using Acme.Workforce.Engagement.Feedback.FeedbackScore;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of feedback score code, run by the project's runner.</summary>
public static class FeedbackTierReportChecks
{
    public static void FeedbackTierReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1707446895);
        var items = new List<FeedbackTier>();
        for (var index = 0; index < 6; index++)
        {
            items.Add(FeedbackTier.NewFeedbackTier($"feedback-tier-{index}", "sample"));
        }
        var report = FeedbackTierReport.BuildFeedbackTierReport("test", items, clock);
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
        FeedbackTierReportCountsEachRecordOnce();
    }
}
