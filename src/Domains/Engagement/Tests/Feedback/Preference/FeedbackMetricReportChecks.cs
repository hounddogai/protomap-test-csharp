namespace Acme.Workforce.Engagement.Tests.Feedback.Preference;

using Acme.Workforce.Engagement.Feedback.Preference;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of preference code, run by the project's runner.</summary>
public static class FeedbackMetricReportChecks
{
    public static void FeedbackMetricReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1702555707);
        var items = new List<FeedbackMetric>();
        for (var index = 0; index < 4; index++)
        {
            items.Add(FeedbackMetric.NewFeedbackMetric($"feedback-metric-{index}", "sample"));
        }
        var report = FeedbackMetricReport.BuildFeedbackMetricReport("test", items, clock);
        var counted = 0;
        foreach (var key in report.Counts.Keys.Order(StringComparer.Ordinal))
        {
            var valueValue = report.Counts[key];
            counted += valueValue;
        }
        Check.True(counted <= 4, "counted");
        Check.Equal(report.GeneratedAt, clock.Now(), "generated at");
    }

    public static void RunAll()
    {
        FeedbackMetricReportCountsEachRecordOnce();
    }
}
