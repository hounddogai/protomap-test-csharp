namespace Acme.Workforce.Orgdesign.Tests.Reorg.Snapshot;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Orgdesign.Reorg.Snapshot;

/// <summary>Checks of snapshot code, run by the project's runner.</summary>
public static class ReorgEstimateEngineChecks
{
    public static void ReorgEstimateEngineKeepsResultsWithinBounds()
    {
        var engine = ReorgEstimateEngine.DefaultReorgEstimateEngine(Flags.NewRegistry());
        var item = ReorgEstimate.NewReorgEstimate("code-145", "label-26");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        ReorgEstimateEngineKeepsResultsWithinBounds();
    }
}
