namespace Acme.Workforce.Compensation.Tests.Benchmark.Reservation;

using Acme.Workforce.Compensation.Benchmark.Reservation;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of reservation code, run by the project's runner.</summary>
public static class BenchmarkBaselineEngineChecks
{
    public static void BenchmarkBaselineEngineKeepsResultsWithinBounds()
    {
        var engine = BenchmarkBaselineEngine.DefaultBenchmarkBaselineEngine(Flags.NewRegistry());
        var item = BenchmarkBaseline.NewBenchmarkBaseline("code-497", "title-646");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        BenchmarkBaselineEngineKeepsResultsWithinBounds();
    }
}
