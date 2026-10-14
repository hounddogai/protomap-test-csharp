namespace Acme.Workforce.Compensation.Tests.Benchmark.BenchmarkRule;

using Acme.Workforce.Compensation.Benchmark.BenchmarkRule;

/// <summary>Checks of benchmark rule code, run by the project's runner.</summary>
public static class BenchmarkRollupChecks
{
    public static void BenchmarkRollupStartsInPlanned()
    {
        var item = BenchmarkRollup.NewBenchmarkRollup("code-598", "name-463");
        Check.Equal(item.Status, BenchmarkRollupStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void BenchmarkRollupMovesToInProgress()
    {
        var item = BenchmarkRollup.NewBenchmarkRollup("code-598", "name-463");
        try
        {
            item.MoveTo(BenchmarkRollupStatus.InProgress);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, BenchmarkRollupStatus.InProgress, "status");
    }

    public static void BenchmarkRollupRejectsMovesFromCancelled()
    {
        var item = BenchmarkRollup.NewBenchmarkRollup("code-598", "name-463");
        item.Status = BenchmarkRollupStatus.Cancelled;
        Check.Fails(() => item.MoveTo(BenchmarkRollupStatus.Planned), "move");
    }

    public static void RunAll()
    {
        BenchmarkRollupStartsInPlanned();
        BenchmarkRollupMovesToInProgress();
        BenchmarkRollupRejectsMovesFromCancelled();
    }
}
