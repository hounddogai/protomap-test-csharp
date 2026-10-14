namespace Acme.Workforce.Compliance.Tests.TrainingDue.Draft;

using Acme.Workforce.Compliance.TrainingDue.Draft;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of draft code, run by the project's runner.</summary>
public static class BatchedTrainingDueDraftReportChecks
{
    public static void BatchedTrainingDueDraftReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1703358282);
        var items = new List<BatchedTrainingDueDraft>();
        for (var index = 0; index < 2; index++)
        {
            items.Add(BatchedTrainingDueDraft.NewBatchedTrainingDueDraft($"batched-training-due-draft-{index}", "sample"));
        }
        var report = BatchedTrainingDueDraftReport.BuildBatchedTrainingDueDraftReport("test", items, clock);
        var counted = 0;
        foreach (var key in report.Counts.Keys.Order(StringComparer.Ordinal))
        {
            var valueValue = report.Counts[key];
            counted += valueValue;
        }
        Check.True(counted <= 2, "counted");
        Check.Equal(report.GeneratedAt, clock.Now(), "generated at");
    }

    public static void RunAll()
    {
        BatchedTrainingDueDraftReportCountsEachRecordOnce();
    }
}
