namespace Acme.Workforce.Compensation.Tests.Benchmark.BenchmarkAdjustment;

using Acme.Workforce.Compensation.Benchmark.BenchmarkAdjustment;

/// <summary>Checks of benchmark adjustment code, run by the project's runner.</summary>
public static class BenchmarkTargetChecks
{
    public static void BenchmarkTargetStartsInIdle()
    {
        var item = BenchmarkTarget.NewBenchmarkTarget("code-264", "name-2");
        Check.Equal(item.Status, BenchmarkTargetStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void BenchmarkTargetMovesToDraining()
    {
        var item = BenchmarkTarget.NewBenchmarkTarget("code-264", "name-2");
        try
        {
            item.MoveTo(BenchmarkTargetStatus.Draining);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, BenchmarkTargetStatus.Draining, "status");
    }

    public static void BenchmarkTargetRejectsMovesFromStopped()
    {
        var item = BenchmarkTarget.NewBenchmarkTarget("code-264", "name-2");
        item.Status = BenchmarkTargetStatus.Stopped;
        Check.Fails(() => item.MoveTo(BenchmarkTargetStatus.Idle), "move");
    }

    public static void RunAll()
    {
        BenchmarkTargetStartsInIdle();
        BenchmarkTargetMovesToDraining();
        BenchmarkTargetRejectsMovesFromStopped();
    }
}
