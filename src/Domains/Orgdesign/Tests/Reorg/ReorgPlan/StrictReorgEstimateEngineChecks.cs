namespace Acme.Workforce.Orgdesign.Tests.Reorg.ReorgPlan;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Orgdesign.Reorg.ReorgPlan;

/// <summary>Checks of reorg plan code, run by the project's runner.</summary>
public static class StrictReorgEstimateEngineChecks
{
    public static void StrictReorgEstimateEngineKeepsResultsWithinBounds()
    {
        var engine = StrictReorgEstimateEngine.DefaultStrictReorgEstimateEngine(Flags.NewRegistry());
        var item = StrictReorgEstimate.NewStrictReorgEstimate("code-28", "name-85");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        StrictReorgEstimateEngineKeepsResultsWithinBounds();
    }
}
