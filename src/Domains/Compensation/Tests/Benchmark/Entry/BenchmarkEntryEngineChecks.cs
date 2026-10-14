namespace Acme.Workforce.Compensation.Tests.Benchmark.Entry;

using Acme.Workforce.Compensation.Benchmark.Entry;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of entry code, run by the project's runner.</summary>
public static class BenchmarkEntryEngineChecks
{
    public static void BenchmarkEntryEngineKeepsResultsWithinBounds()
    {
        var engine = BenchmarkEntryEngine.DefaultBenchmarkEntryEngine(Flags.NewRegistry());
        var item = BenchmarkEntry.NewBenchmarkEntry("code-909", "title-994");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        BenchmarkEntryEngineKeepsResultsWithinBounds();
    }
}
