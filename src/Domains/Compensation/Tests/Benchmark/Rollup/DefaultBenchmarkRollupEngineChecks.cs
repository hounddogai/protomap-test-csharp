namespace Acme.Workforce.Compensation.Tests.Benchmark.Rollup;

using Acme.Workforce.Compensation.Benchmark.Rollup;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of rollup code, run by the project's runner.</summary>
public static class DefaultBenchmarkRollupEngineChecks
{
    public static void DefaultBenchmarkRollupEngineKeepsResultsWithinBounds()
    {
        var engine = DefaultBenchmarkRollupEngine.DefaultDefaultBenchmarkRollupEngine(Flags.NewRegistry());
        var item = DefaultBenchmarkRollup.NewDefaultBenchmarkRollup("code-721", "label-552");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        DefaultBenchmarkRollupEngineKeepsResultsWithinBounds();
    }
}
