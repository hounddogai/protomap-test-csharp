namespace Acme.Workforce.Compensation.Tests.Benchmark.BenchmarkSummary;

using Acme.Workforce.Compensation.Benchmark.BenchmarkSummary;

/// <summary>Checks of benchmark summary code, run by the project's runner.</summary>
public static class LenientBenchmarkManifestEngineChecks
{
    public static void LenientBenchmarkManifestEngineKeepsResultsWithinBounds()
    {
        var engine = LenientBenchmarkManifestEngine.DefaultLenientBenchmarkManifestEngine();
        var item = LenientBenchmarkManifest.NewLenientBenchmarkManifest("code-383", "name-521");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        LenientBenchmarkManifestEngineKeepsResultsWithinBounds();
    }
}
