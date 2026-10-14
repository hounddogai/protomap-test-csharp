namespace Acme.Workforce.Compensation.Tests.Benchmark.Rollup;

using Acme.Workforce.Compensation.Benchmark.Rollup;

/// <summary>Checks of rollup code, run by the project's runner.</summary>
public static class DefaultBenchmarkRollupChecks
{
    public static void DefaultBenchmarkRollupStartsInPlanned()
    {
        var item = DefaultBenchmarkRollup.NewDefaultBenchmarkRollup("code-721", "label-552");
        Check.Equal(item.Status, DefaultBenchmarkRollupStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void DefaultBenchmarkRollupMovesToScheduled()
    {
        var item = DefaultBenchmarkRollup.NewDefaultBenchmarkRollup("code-721", "label-552");
        try
        {
            item.MoveTo(DefaultBenchmarkRollupStatus.Scheduled);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, DefaultBenchmarkRollupStatus.Scheduled, "status");
    }

    public static void DefaultBenchmarkRollupRejectsMovesFromCancelled()
    {
        var item = DefaultBenchmarkRollup.NewDefaultBenchmarkRollup("code-721", "label-552");
        item.Status = DefaultBenchmarkRollupStatus.Cancelled;
        Check.Fails(() => item.MoveTo(DefaultBenchmarkRollupStatus.Planned), "move");
    }

    public static void RunAll()
    {
        DefaultBenchmarkRollupStartsInPlanned();
        DefaultBenchmarkRollupMovesToScheduled();
        DefaultBenchmarkRollupRejectsMovesFromCancelled();
    }
}
