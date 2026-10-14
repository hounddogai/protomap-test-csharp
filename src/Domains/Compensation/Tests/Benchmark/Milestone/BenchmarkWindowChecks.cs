namespace Acme.Workforce.Compensation.Tests.Benchmark.Milestone;

using Acme.Workforce.Compensation.Benchmark.Milestone;

/// <summary>Checks of milestone code, run by the project's runner.</summary>
public static class BenchmarkWindowChecks
{
    public static void BenchmarkWindowStartsInProposed()
    {
        var item = BenchmarkWindow.NewBenchmarkWindow("code-866", "name-287");
        Check.Equal(item.Status, BenchmarkWindowStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void BenchmarkWindowMovesToRejected()
    {
        var item = BenchmarkWindow.NewBenchmarkWindow("code-866", "name-287");
        try
        {
            item.MoveTo(BenchmarkWindowStatus.Rejected);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, BenchmarkWindowStatus.Rejected, "status");
    }

    public static void BenchmarkWindowRejectsMovesFromWithdrawn()
    {
        var item = BenchmarkWindow.NewBenchmarkWindow("code-866", "name-287");
        item.Status = BenchmarkWindowStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(BenchmarkWindowStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        BenchmarkWindowStartsInProposed();
        BenchmarkWindowMovesToRejected();
        BenchmarkWindowRejectsMovesFromWithdrawn();
    }
}
