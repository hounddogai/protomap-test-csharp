namespace Acme.Workforce.Compensation.Tests.Benchmark.Metric;

using Acme.Workforce.Compensation.Benchmark.Metric;

/// <summary>Checks of metric code, run by the project's runner.</summary>
public static class BatchedBenchmarkSnapshotChecks
{
    public static void BatchedBenchmarkSnapshotStartsInCreated()
    {
        var item = BatchedBenchmarkSnapshot.NewBatchedBenchmarkSnapshot("code-112", "name-454");
        Check.Equal(item.Status, BatchedBenchmarkSnapshotStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void BatchedBenchmarkSnapshotMovesToValidated()
    {
        var item = BatchedBenchmarkSnapshot.NewBatchedBenchmarkSnapshot("code-112", "name-454");
        try
        {
            item.MoveTo(BatchedBenchmarkSnapshotStatus.Validated);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, BatchedBenchmarkSnapshotStatus.Validated, "status");
    }

    public static void BatchedBenchmarkSnapshotRejectsMovesFromRetired()
    {
        var item = BatchedBenchmarkSnapshot.NewBatchedBenchmarkSnapshot("code-112", "name-454");
        item.Status = BatchedBenchmarkSnapshotStatus.Retired;
        Check.Fails(() => item.MoveTo(BatchedBenchmarkSnapshotStatus.Created), "move");
    }

    public static void RunAll()
    {
        BatchedBenchmarkSnapshotStartsInCreated();
        BatchedBenchmarkSnapshotMovesToValidated();
        BatchedBenchmarkSnapshotRejectsMovesFromRetired();
    }
}
