namespace Acme.Workforce.Orgdesign.Tests.Reorg.ReorgClaim;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Orgdesign.Reorg.ReorgClaim;

/// <summary>Checks of reorg claim code, run by the project's runner.</summary>
public static class RollingReorgSummaryEngineChecks
{
    public static void RollingReorgSummaryEngineKeepsResultsWithinBounds()
    {
        var engine = RollingReorgSummaryEngine.DefaultRollingReorgSummaryEngine(Flags.NewRegistry());
        var item = RollingReorgSummary.NewRollingReorgSummary("code-752", "name-175");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        RollingReorgSummaryEngineKeepsResultsWithinBounds();
    }
}
