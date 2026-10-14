namespace Acme.Workforce.Compensation.Tests.Benchmark.Proposal;

using Acme.Workforce.Compensation.Benchmark.Proposal;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of proposal code, run by the project's runner.</summary>
public static class TieredBenchmarkSummaryEngineChecks
{
    public static void TieredBenchmarkSummaryEngineKeepsResultsWithinBounds()
    {
        var engine = TieredBenchmarkSummaryEngine.DefaultTieredBenchmarkSummaryEngine(Flags.NewRegistry());
        var item = TieredBenchmarkSummary.NewTieredBenchmarkSummary("code-943", "name-766");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        TieredBenchmarkSummaryEngineKeepsResultsWithinBounds();
    }
}
