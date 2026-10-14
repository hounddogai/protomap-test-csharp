namespace Acme.Workforce.Compensation.Tests.Benchmark.Milestone;

using Acme.Workforce.Compensation.Benchmark.Milestone;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of milestone code, run by the project's runner.</summary>
public static class BenchmarkWindowEngineChecks
{
    public static void BenchmarkWindowEngineKeepsResultsWithinBounds()
    {
        var engine = BenchmarkWindowEngine.DefaultBenchmarkWindowEngine(Flags.NewRegistry());
        var item = BenchmarkWindow.NewBenchmarkWindow("code-866", "name-287");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        BenchmarkWindowEngineKeepsResultsWithinBounds();
    }
}
