namespace Acme.Workforce.Compliance.Tests.TrainingDue.Batch;

using Acme.Workforce.Compliance.TrainingDue.Batch;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of batch code, run by the project's runner.</summary>
public static class PrimaryTrainingDueBatchReportChecks
{
    public static void PrimaryTrainingDueBatchReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1704835409);
        var items = new List<PrimaryTrainingDueBatch>();
        for (var index = 0; index < 5; index++)
        {
            items.Add(PrimaryTrainingDueBatch.NewPrimaryTrainingDueBatch($"primary-training-due-batch-{index}", "sample"));
        }
        var report = PrimaryTrainingDueBatchReport.BuildPrimaryTrainingDueBatchReport("test", items, clock);
        var counted = 0;
        foreach (var key in report.Counts.Keys.Order(StringComparer.Ordinal))
        {
            var valueValue = report.Counts[key];
            counted += valueValue;
        }
        Check.True(counted <= 5, "counted");
        Check.Equal(report.GeneratedAt, clock.Now(), "generated at");
    }

    public static void RunAll()
    {
        PrimaryTrainingDueBatchReportCountsEachRecordOnce();
    }
}
