namespace Acme.Workforce.Compensation.Tests.Benchmark.Audit;

using Acme.Workforce.Compensation.Benchmark.Audit;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of audit code, run by the project's runner.</summary>
public static class BenchmarkForecastEngineChecks
{
    public static void BenchmarkForecastEngineKeepsResultsWithinBounds()
    {
        var engine = BenchmarkForecastEngine.DefaultBenchmarkForecastEngine(Flags.NewRegistry());
        var item = BenchmarkForecast.NewBenchmarkForecast("code-903", "label-470");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        BenchmarkForecastEngineKeepsResultsWithinBounds();
    }
}
