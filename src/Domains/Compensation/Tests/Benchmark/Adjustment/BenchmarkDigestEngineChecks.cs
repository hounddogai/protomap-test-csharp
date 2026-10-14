namespace Acme.Workforce.Compensation.Tests.Benchmark.Adjustment;

using Acme.Workforce.Compensation.Benchmark.Adjustment;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of adjustment code, run by the project's runner.</summary>
public static class BenchmarkDigestEngineChecks
{
    public static void BenchmarkDigestEngineKeepsResultsWithinBounds()
    {
        var engine = BenchmarkDigestEngine.DefaultBenchmarkDigestEngine(Flags.NewRegistry());
        var item = BenchmarkDigest.NewBenchmarkDigest("code-48", "title-828");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        BenchmarkDigestEngineKeepsResultsWithinBounds();
    }
}
