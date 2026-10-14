namespace Acme.Workforce.Compensation.Tests.Benchmark.Score;

using Acme.Workforce.Compensation.Benchmark.Score;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of score code, run by the project's runner.</summary>
public static class TieredBenchmarkProfileEngineChecks
{
    public static void TieredBenchmarkProfileEngineKeepsResultsWithinBounds()
    {
        var engine = TieredBenchmarkProfileEngine.DefaultTieredBenchmarkProfileEngine(Flags.NewRegistry());
        var item = TieredBenchmarkProfile.NewTieredBenchmarkProfile("code-6", "name-357");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        TieredBenchmarkProfileEngineKeepsResultsWithinBounds();
    }
}
