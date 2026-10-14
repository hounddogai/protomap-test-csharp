namespace Acme.Workforce.Compensation.Tests.Benchmark.BenchmarkAdjustment;

using Acme.Workforce.Compensation.Benchmark.BenchmarkAdjustment;

/// <summary>Checks of benchmark adjustment code, run by the project's runner.</summary>
public static class TieredBenchmarkAdjustmentChecks
{
    public static void TieredBenchmarkAdjustmentStartsInIdle()
    {
        var item = TieredBenchmarkAdjustment.NewTieredBenchmarkAdjustment("code-659", "name-811");
        Check.Equal(item.Status, TieredBenchmarkAdjustmentStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void TieredBenchmarkAdjustmentMovesToStopped()
    {
        var item = TieredBenchmarkAdjustment.NewTieredBenchmarkAdjustment("code-659", "name-811");
        try
        {
            item.MoveTo(TieredBenchmarkAdjustmentStatus.Stopped);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, TieredBenchmarkAdjustmentStatus.Stopped, "status");
    }

    public static void TieredBenchmarkAdjustmentRejectsMovesFromStopped()
    {
        var item = TieredBenchmarkAdjustment.NewTieredBenchmarkAdjustment("code-659", "name-811");
        item.Status = TieredBenchmarkAdjustmentStatus.Stopped;
        Check.Fails(() => item.MoveTo(TieredBenchmarkAdjustmentStatus.Idle), "move");
    }

    public static void RunAll()
    {
        TieredBenchmarkAdjustmentStartsInIdle();
        TieredBenchmarkAdjustmentMovesToStopped();
        TieredBenchmarkAdjustmentRejectsMovesFromStopped();
    }
}
