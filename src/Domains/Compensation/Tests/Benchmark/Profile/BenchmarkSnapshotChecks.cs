namespace Acme.Workforce.Compensation.Tests.Benchmark.Profile;

using Acme.Workforce.Compensation.Benchmark.Profile;

/// <summary>Checks of profile code, run by the project's runner.</summary>
public static class BenchmarkSnapshotChecks
{
    public static void BenchmarkSnapshotStartsInOpen()
    {
        var item = BenchmarkSnapshot.NewBenchmarkSnapshot("code-122", "name-445");
        Check.Equal(item.Status, BenchmarkSnapshotStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void BenchmarkSnapshotMovesToResolved()
    {
        var item = BenchmarkSnapshot.NewBenchmarkSnapshot("code-122", "name-445");
        try
        {
            item.MoveTo(BenchmarkSnapshotStatus.Resolved);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, BenchmarkSnapshotStatus.Resolved, "status");
    }

    public static void BenchmarkSnapshotRejectsMovesFromArchived()
    {
        var item = BenchmarkSnapshot.NewBenchmarkSnapshot("code-122", "name-445");
        item.Status = BenchmarkSnapshotStatus.Archived;
        Check.Fails(() => item.MoveTo(BenchmarkSnapshotStatus.Open), "move");
    }

    public static void RunAll()
    {
        BenchmarkSnapshotStartsInOpen();
        BenchmarkSnapshotMovesToResolved();
        BenchmarkSnapshotRejectsMovesFromArchived();
    }
}
