namespace Acme.Workforce.Compensation.Tests.Benchmark.Allocation;

using Acme.Workforce.Compensation.Benchmark.Allocation;

/// <summary>Checks of allocation code, run by the project's runner.</summary>
public static class BenchmarkCheckpointEngineChecks
{
    public static void BenchmarkCheckpointEngineKeepsResultsWithinBounds()
    {
        var engine = BenchmarkCheckpointEngine.DefaultBenchmarkCheckpointEngine();
        var item = BenchmarkCheckpoint.NewBenchmarkCheckpoint("code-669", "name-422");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        BenchmarkCheckpointEngineKeepsResultsWithinBounds();
    }
}
