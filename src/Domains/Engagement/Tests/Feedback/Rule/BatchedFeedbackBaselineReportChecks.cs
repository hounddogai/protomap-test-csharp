namespace Acme.Workforce.Engagement.Tests.Feedback.Rule;

using Acme.Workforce.Engagement.Feedback.Rule;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of rule code, run by the project's runner.</summary>
public static class BatchedFeedbackBaselineReportChecks
{
    public static void BatchedFeedbackBaselineReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1701096902);
        var items = new List<BatchedFeedbackBaseline>();
        for (var index = 0; index < 3; index++)
        {
            items.Add(BatchedFeedbackBaseline.NewBatchedFeedbackBaseline($"batched-feedback-baseline-{index}", "sample"));
        }
        var report = BatchedFeedbackBaselineReport.BuildBatchedFeedbackBaselineReport("test", items, clock);
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
        BatchedFeedbackBaselineReportCountsEachRecordOnce();
    }
}
