namespace Acme.Workforce.Compensation.Tests.Benchmark.Metric;

using Acme.Workforce.Compensation.Benchmark.Metric;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of metric code, run by the project's runner.</summary>
public static class ManualBenchmarkMetricEngineChecks
{
    public static void ManualBenchmarkMetricEngineKeepsResultsWithinBounds()
    {
        var engine = ManualBenchmarkMetricEngine.DefaultManualBenchmarkMetricEngine(Flags.NewRegistry());
        var item = ManualBenchmarkMetric.NewManualBenchmarkMetric("code-971", "label-834");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        ManualBenchmarkMetricEngineKeepsResultsWithinBounds();
    }
}
