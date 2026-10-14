namespace Acme.Workforce.Compensation.Tests.Benchmark.Rule;

using Acme.Workforce.Compensation.Benchmark.Rule;

/// <summary>Checks of rule code, run by the project's runner.</summary>
public static class WeightedBenchmarkMappingChecks
{
    public static void WeightedBenchmarkMappingStartsInIdle()
    {
        var item = WeightedBenchmarkMapping.NewWeightedBenchmarkMapping("code-565", "title-254");
        Check.Equal(item.Status, WeightedBenchmarkMappingStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void WeightedBenchmarkMappingMovesToWarming()
    {
        var item = WeightedBenchmarkMapping.NewWeightedBenchmarkMapping("code-565", "title-254");
        try
        {
            item.MoveTo(WeightedBenchmarkMappingStatus.Warming);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, WeightedBenchmarkMappingStatus.Warming, "status");
    }

    public static void WeightedBenchmarkMappingRejectsMovesFromStopped()
    {
        var item = WeightedBenchmarkMapping.NewWeightedBenchmarkMapping("code-565", "title-254");
        item.Status = WeightedBenchmarkMappingStatus.Stopped;
        Check.Fails(() => item.MoveTo(WeightedBenchmarkMappingStatus.Idle), "move");
    }

    public static void RunAll()
    {
        WeightedBenchmarkMappingStartsInIdle();
        WeightedBenchmarkMappingMovesToWarming();
        WeightedBenchmarkMappingRejectsMovesFromStopped();
    }
}
