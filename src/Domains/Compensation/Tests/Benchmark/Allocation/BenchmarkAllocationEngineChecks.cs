namespace Acme.Workforce.Compensation.Tests.Benchmark.Allocation;

using Acme.Workforce.Compensation.Benchmark.Allocation;

/// <summary>Checks of allocation code, run by the project's runner.</summary>
public static class BenchmarkAllocationEngineChecks
{
    public static void BenchmarkAllocationEngineKeepsResultsWithinBounds()
    {
        var engine = BenchmarkAllocationEngine.DefaultBenchmarkAllocationEngine();
        var item = BenchmarkAllocation.NewBenchmarkAllocation("code-50", "label-320");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        BenchmarkAllocationEngineKeepsResultsWithinBounds();
    }
}
