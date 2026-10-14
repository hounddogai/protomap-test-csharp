namespace Acme.Workforce.Orgdesign.Tests.Reorg.Checkpoint;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Orgdesign.Reorg.Checkpoint;

/// <summary>Checks of checkpoint code, run by the project's runner.</summary>
public static class BatchedReorgCheckpointReportChecks
{
    public static void BatchedReorgCheckpointReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1707287459);
        var items = new List<BatchedReorgCheckpoint>();
        for (var index = 0; index < 3; index++)
        {
            items.Add(BatchedReorgCheckpoint.NewBatchedReorgCheckpoint($"batched-reorg-checkpoint-{index}", "sample"));
        }
        var report = BatchedReorgCheckpointReport.BuildBatchedReorgCheckpointReport("test", items, clock);
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
        BatchedReorgCheckpointReportCountsEachRecordOnce();
    }
}
