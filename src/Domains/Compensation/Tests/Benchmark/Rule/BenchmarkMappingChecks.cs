namespace Acme.Workforce.Compensation.Tests.Benchmark.Rule;

using Acme.Workforce.Compensation.Benchmark.Rule;

/// <summary>Checks of rule code, run by the project's runner.</summary>
public static class BenchmarkMappingChecks
{
    public static void BenchmarkMappingStartsInIdle()
    {
        var item = BenchmarkMapping.NewBenchmarkMapping("code-594", "name-416");
        Check.Equal(item.Status, BenchmarkMappingStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void BenchmarkMappingMovesToWarming()
    {
        var item = BenchmarkMapping.NewBenchmarkMapping("code-594", "name-416");
        try
        {
            item.MoveTo(BenchmarkMappingStatus.Warming);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, BenchmarkMappingStatus.Warming, "status");
    }

    public static void BenchmarkMappingRejectsMovesFromStopped()
    {
        var item = BenchmarkMapping.NewBenchmarkMapping("code-594", "name-416");
        item.Status = BenchmarkMappingStatus.Stopped;
        Check.Fails(() => item.MoveTo(BenchmarkMappingStatus.Idle), "move");
    }

    public static void RunAll()
    {
        BenchmarkMappingStartsInIdle();
        BenchmarkMappingMovesToWarming();
        BenchmarkMappingRejectsMovesFromStopped();
    }
}
