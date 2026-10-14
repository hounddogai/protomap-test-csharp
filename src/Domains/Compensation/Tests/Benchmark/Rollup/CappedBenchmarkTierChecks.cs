namespace Acme.Workforce.Compensation.Tests.Benchmark.Rollup;

using Acme.Workforce.Compensation.Benchmark.Rollup;

/// <summary>Checks of rollup code, run by the project's runner.</summary>
public static class CappedBenchmarkTierChecks
{
    public static void CappedBenchmarkTierStartsInIdle()
    {
        var item = CappedBenchmarkTier.NewCappedBenchmarkTier("code-809", "label-3");
        Check.Equal(item.Status, CappedBenchmarkTierStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void CappedBenchmarkTierMovesToWarming()
    {
        var item = CappedBenchmarkTier.NewCappedBenchmarkTier("code-809", "label-3");
        try
        {
            item.MoveTo(CappedBenchmarkTierStatus.Warming);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, CappedBenchmarkTierStatus.Warming, "status");
    }

    public static void CappedBenchmarkTierRejectsMovesFromStopped()
    {
        var item = CappedBenchmarkTier.NewCappedBenchmarkTier("code-809", "label-3");
        item.Status = CappedBenchmarkTierStatus.Stopped;
        Check.Fails(() => item.MoveTo(CappedBenchmarkTierStatus.Idle), "move");
    }

    public static void RunAll()
    {
        CappedBenchmarkTierStartsInIdle();
        CappedBenchmarkTierMovesToWarming();
        CappedBenchmarkTierRejectsMovesFromStopped();
    }
}
