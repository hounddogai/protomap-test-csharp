namespace Acme.Workforce.Absence.Tests.Accrual.Notice;

using Acme.Workforce.Absence.Accrual.Notice;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of notice code, run by the project's runner.</summary>
public static class BatchedAccrualCheckpointReportChecks
{
    public static void BatchedAccrualCheckpointReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1709504786);
        var items = new List<BatchedAccrualCheckpoint>();
        for (var index = 0; index < 6; index++)
        {
            items.Add(BatchedAccrualCheckpoint.NewBatchedAccrualCheckpoint($"batched-accrual-checkpoint-{index}", "sample"));
        }
        var report = BatchedAccrualCheckpointReport.BuildBatchedAccrualCheckpointReport("test", items, clock);
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
        BatchedAccrualCheckpointReportCountsEachRecordOnce();
    }
}
