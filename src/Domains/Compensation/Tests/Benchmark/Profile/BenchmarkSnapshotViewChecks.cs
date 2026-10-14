namespace Acme.Workforce.Compensation.Tests.Benchmark.Profile;

using Acme.Workforce.Compensation.Benchmark.Profile;

/// <summary>Checks of profile code, run by the project's runner.</summary>
public static class BenchmarkSnapshotViewChecks
{
    public static void BenchmarkSnapshotViewShowsTheStatusInCapitals()
    {
        var item = BenchmarkSnapshot.NewBenchmarkSnapshot("code-122", "name-445");
        var view = BenchmarkSnapshotView.ToBenchmarkSnapshotView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(BenchmarkSnapshotView.ToBenchmarkSnapshotViews(new List<BenchmarkSnapshot> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        BenchmarkSnapshotViewShowsTheStatusInCapitals();
    }
}
