namespace Acme.Workforce.Compliance.Tests.TrainingDue.DueTier;

using Acme.Workforce.Compliance.TrainingDue.DueTier;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of due tier code, run by the project's runner.</summary>
public static class BatchedTrainingDueNoticeReportChecks
{
    public static void BatchedTrainingDueNoticeReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1708694766);
        var items = new List<BatchedTrainingDueNotice>();
        for (var index = 0; index < 3; index++)
        {
            items.Add(BatchedTrainingDueNotice.NewBatchedTrainingDueNotice($"batched-training-due-notice-{index}", "sample"));
        }
        var report = BatchedTrainingDueNoticeReport.BuildBatchedTrainingDueNoticeReport("test", items, clock);
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
        BatchedTrainingDueNoticeReportCountsEachRecordOnce();
    }
}
