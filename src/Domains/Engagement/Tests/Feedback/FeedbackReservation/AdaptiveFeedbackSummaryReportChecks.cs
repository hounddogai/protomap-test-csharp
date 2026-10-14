namespace Acme.Workforce.Engagement.Tests.Feedback.FeedbackReservation;

using Acme.Workforce.Engagement.Feedback.FeedbackReservation;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of feedback reservation code, run by the project's runner.</summary>
public static class AdaptiveFeedbackSummaryReportChecks
{
    public static void AdaptiveFeedbackSummaryReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1701116240);
        var items = new List<AdaptiveFeedbackSummary>();
        for (var index = 0; index < 6; index++)
        {
            items.Add(AdaptiveFeedbackSummary.NewAdaptiveFeedbackSummary($"adaptive-feedback-summary-{index}", "sample"));
        }
        var report = AdaptiveFeedbackSummaryReport.BuildAdaptiveFeedbackSummaryReport("test", items, clock);
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
        AdaptiveFeedbackSummaryReportCountsEachRecordOnce();
    }
}
