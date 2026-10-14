namespace Acme.Workforce.Compensation.Tests.Benchmark.Summary;

using Acme.Workforce.Compensation.Benchmark.Summary;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of summary code, run by the project's runner.</summary>
public static class BenchmarkSummaryEngineChecks
{
    public static void BenchmarkSummaryEngineKeepsResultsWithinBounds()
    {
        var engine = BenchmarkSummaryEngine.DefaultBenchmarkSummaryEngine(Flags.NewRegistry());
        var item = BenchmarkSummary.NewBenchmarkSummary("code-520", "label-137");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        BenchmarkSummaryEngineKeepsResultsWithinBounds();
    }
}
