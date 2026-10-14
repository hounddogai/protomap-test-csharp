namespace Acme.Workforce.Compensation.Tests.Benchmark.Baseline;

using Acme.Workforce.Compensation.Benchmark.Baseline;

/// <summary>Checks of baseline code, run by the project's runner.</summary>
public static class FallbackBenchmarkRuleEngineChecks
{
    public static void FallbackBenchmarkRuleEngineKeepsResultsWithinBounds()
    {
        var engine = FallbackBenchmarkRuleEngine.DefaultFallbackBenchmarkRuleEngine();
        var item = FallbackBenchmarkRule.NewFallbackBenchmarkRule("code-516", "label-470");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        FallbackBenchmarkRuleEngineKeepsResultsWithinBounds();
    }
}
