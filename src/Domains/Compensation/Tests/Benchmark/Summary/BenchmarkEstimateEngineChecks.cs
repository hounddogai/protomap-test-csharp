namespace Acme.Workforce.Compensation.Tests.Benchmark.Summary;

using Acme.Workforce.Compensation.Benchmark.Summary;

/// <summary>Checks of summary code, run by the project's runner.</summary>
public static class BenchmarkEstimateEngineChecks
{
    public static void BenchmarkEstimateEngineKeepsResultsWithinBounds()
    {
        var engine = BenchmarkEstimateEngine.DefaultBenchmarkEstimateEngine();
        var item = BenchmarkEstimate.NewBenchmarkEstimate("code-936", "title-175");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        BenchmarkEstimateEngineKeepsResultsWithinBounds();
    }
}
