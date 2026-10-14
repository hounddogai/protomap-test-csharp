namespace Acme.Workforce.Compensation.Tests.Benchmark.Allocation;

using Acme.Workforce.Compensation.Benchmark.Allocation;

/// <summary>Checks of allocation code, run by the project's runner.</summary>
public static class BenchmarkBatchEngineChecks
{
    public static void BenchmarkBatchEngineKeepsResultsWithinBounds()
    {
        var engine = BenchmarkBatchEngine.DefaultBenchmarkBatchEngine();
        var item = BenchmarkBatch.NewBenchmarkBatch("code-559", "label-410");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        BenchmarkBatchEngineKeepsResultsWithinBounds();
    }
}
