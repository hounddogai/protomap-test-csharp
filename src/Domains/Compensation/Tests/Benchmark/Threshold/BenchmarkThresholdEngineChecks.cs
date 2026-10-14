namespace Acme.Workforce.Compensation.Tests.Benchmark.Threshold;

using Acme.Workforce.Compensation.Benchmark.Threshold;

/// <summary>Checks of threshold code, run by the project's runner.</summary>
public static class BenchmarkThresholdEngineChecks
{
    public static void BenchmarkThresholdEngineKeepsResultsWithinBounds()
    {
        var engine = BenchmarkThresholdEngine.DefaultBenchmarkThresholdEngine();
        var item = BenchmarkThreshold.NewBenchmarkThreshold("code-628", "label-144");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        BenchmarkThresholdEngineKeepsResultsWithinBounds();
    }
}
