namespace Acme.Workforce.Compensation.Tests.Benchmark.Metric;

using Acme.Workforce.Compensation.Benchmark.Metric;

/// <summary>Checks of metric code, run by the project's runner.</summary>
public static class BatchedBenchmarkSnapshotViewChecks
{
    public static void BatchedBenchmarkSnapshotViewShowsTheStatusInCapitals()
    {
        var item = BatchedBenchmarkSnapshot.NewBatchedBenchmarkSnapshot("code-112", "name-454");
        var view = BatchedBenchmarkSnapshotView.ToBatchedBenchmarkSnapshotView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(BatchedBenchmarkSnapshotView.ToBatchedBenchmarkSnapshotViews(new List<BatchedBenchmarkSnapshot> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        BatchedBenchmarkSnapshotViewShowsTheStatusInCapitals();
    }
}
