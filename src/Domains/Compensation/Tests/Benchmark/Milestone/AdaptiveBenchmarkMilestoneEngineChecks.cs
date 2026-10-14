namespace Acme.Workforce.Compensation.Tests.Benchmark.Milestone;

using Acme.Workforce.Compensation.Benchmark.Milestone;

/// <summary>Checks of milestone code, run by the project's runner.</summary>
public static class AdaptiveBenchmarkMilestoneEngineChecks
{
    public static void AdaptiveBenchmarkMilestoneEngineKeepsResultsWithinBounds()
    {
        var engine = AdaptiveBenchmarkMilestoneEngine.DefaultAdaptiveBenchmarkMilestoneEngine();
        var item = AdaptiveBenchmarkMilestone.NewAdaptiveBenchmarkMilestone("code-682", "name-794");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        AdaptiveBenchmarkMilestoneEngineKeepsResultsWithinBounds();
    }
}
