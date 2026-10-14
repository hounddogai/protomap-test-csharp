namespace Acme.Workforce.Compensation.Tests.Benchmark.Allocation;

using Acme.Workforce.Compensation.Benchmark.Allocation;

/// <summary>Checks of allocation code, run by the project's runner.</summary>
public static class BenchmarkCheckpointChecks
{
    public static void BenchmarkCheckpointStartsInOpen()
    {
        var item = BenchmarkCheckpoint.NewBenchmarkCheckpoint("code-669", "name-422");
        Check.Equal(item.Status, BenchmarkCheckpointStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void BenchmarkCheckpointMovesToResolved()
    {
        var item = BenchmarkCheckpoint.NewBenchmarkCheckpoint("code-669", "name-422");
        try
        {
            item.MoveTo(BenchmarkCheckpointStatus.Resolved);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, BenchmarkCheckpointStatus.Resolved, "status");
    }

    public static void BenchmarkCheckpointRejectsMovesFromArchived()
    {
        var item = BenchmarkCheckpoint.NewBenchmarkCheckpoint("code-669", "name-422");
        item.Status = BenchmarkCheckpointStatus.Archived;
        Check.Fails(() => item.MoveTo(BenchmarkCheckpointStatus.Open), "move");
    }

    public static void RunAll()
    {
        BenchmarkCheckpointStartsInOpen();
        BenchmarkCheckpointMovesToResolved();
        BenchmarkCheckpointRejectsMovesFromArchived();
    }
}
