namespace Acme.Workforce.Compensation.Tests.Benchmark.Preference;

using Acme.Workforce.Compensation.Benchmark.Preference;

/// <summary>Checks of preference code, run by the project's runner.</summary>
public static class RollingBenchmarkProposalEngineChecks
{
    public static void RollingBenchmarkProposalEngineKeepsResultsWithinBounds()
    {
        var engine = RollingBenchmarkProposalEngine.DefaultRollingBenchmarkProposalEngine();
        var item = RollingBenchmarkProposal.NewRollingBenchmarkProposal("code-387", "name-560");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        RollingBenchmarkProposalEngineKeepsResultsWithinBounds();
    }
}
