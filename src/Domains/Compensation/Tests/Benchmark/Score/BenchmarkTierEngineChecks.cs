namespace Acme.Workforce.Compensation.Tests.Benchmark.Score;

using Acme.Workforce.Compensation.Benchmark.Score;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of score code, run by the project's runner.</summary>
public static class BenchmarkTierEngineChecks
{
    public static void BenchmarkTierEngineKeepsResultsWithinBounds()
    {
        var engine = BenchmarkTierEngine.DefaultBenchmarkTierEngine(Flags.NewRegistry());
        var item = BenchmarkTier.NewBenchmarkTier("code-577", "title-933");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        BenchmarkTierEngineKeepsResultsWithinBounds();
    }
}
