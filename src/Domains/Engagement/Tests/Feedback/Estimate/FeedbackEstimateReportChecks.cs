namespace Acme.Workforce.Engagement.Tests.Feedback.Estimate;

using Acme.Workforce.Engagement.Feedback.Estimate;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of estimate code, run by the project's runner.</summary>
public static class FeedbackEstimateReportChecks
{
    public static void FeedbackEstimateReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1705407717);
        var items = new List<FeedbackEstimate>();
        for (var index = 0; index < 3; index++)
        {
            items.Add(FeedbackEstimate.NewFeedbackEstimate($"feedback-estimate-{index}", "sample"));
        }
        var report = FeedbackEstimateReport.BuildFeedbackEstimateReport("test", items, clock);
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
        FeedbackEstimateReportCountsEachRecordOnce();
    }
}
