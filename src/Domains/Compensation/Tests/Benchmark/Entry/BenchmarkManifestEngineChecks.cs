namespace Acme.Workforce.Compensation.Tests.Benchmark.Entry;

using Acme.Workforce.Compensation.Benchmark.Entry;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of entry code, run by the project's runner.</summary>
public static class BenchmarkManifestEngineChecks
{
    public static void BenchmarkManifestEngineKeepsResultsWithinBounds()
    {
        var engine = BenchmarkManifestEngine.DefaultBenchmarkManifestEngine(Flags.NewRegistry());
        var item = BenchmarkManifest.NewBenchmarkManifest("code-302", "label-773");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        BenchmarkManifestEngineKeepsResultsWithinBounds();
    }
}
