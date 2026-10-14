namespace Acme.Workforce.Compensation.Tests.Benchmark.Summary;

using Acme.Workforce.Compensation.Benchmark.Summary;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of summary code, run by the project's runner.</summary>
public static class BenchmarkOverrideEngineChecks
{
    public static void BenchmarkOverrideEngineKeepsResultsWithinBounds()
    {
        var engine = BenchmarkOverrideEngine.DefaultBenchmarkOverrideEngine(Flags.NewRegistry());
        var item = BenchmarkOverride.NewBenchmarkOverride("code-331", "title-341");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        BenchmarkOverrideEngineKeepsResultsWithinBounds();
    }
}
