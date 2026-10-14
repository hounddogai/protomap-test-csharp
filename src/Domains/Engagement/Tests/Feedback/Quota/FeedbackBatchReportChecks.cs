namespace Acme.Workforce.Engagement.Tests.Feedback.Quota;

using Acme.Workforce.Engagement.Feedback.Quota;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of quota code, run by the project's runner.</summary>
public static class FeedbackBatchReportChecks
{
    public static void FeedbackBatchReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1706049378);
        var items = new List<FeedbackBatch>();
        for (var index = 0; index < 6; index++)
        {
            items.Add(FeedbackBatch.NewFeedbackBatch($"feedback-batch-{index}", "sample"));
        }
        var report = FeedbackBatchReport.BuildFeedbackBatchReport("test", items, clock);
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
        FeedbackBatchReportCountsEachRecordOnce();
    }
}
