namespace Acme.Workforce.Compensation.Tests.Benchmark.BenchmarkSummary;

using Acme.Workforce.Compensation.Benchmark.BenchmarkSummary;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of benchmark summary code, run by the project's runner.</summary>
public static class ManualBenchmarkAllocationEngineChecks
{
    public static void ManualBenchmarkAllocationEngineKeepsResultsWithinBounds()
    {
        var engine = ManualBenchmarkAllocationEngine.DefaultManualBenchmarkAllocationEngine(Flags.NewRegistry());
        var item = ManualBenchmarkAllocation.NewManualBenchmarkAllocation("code-36", "label-351");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        ManualBenchmarkAllocationEngineKeepsResultsWithinBounds();
    }
}
