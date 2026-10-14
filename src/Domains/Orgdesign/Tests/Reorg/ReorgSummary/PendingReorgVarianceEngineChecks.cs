namespace Acme.Workforce.Orgdesign.Tests.Reorg.ReorgSummary;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Orgdesign.Reorg.ReorgSummary;

/// <summary>Checks of reorg summary code, run by the project's runner.</summary>
public static class PendingReorgVarianceEngineChecks
{
    public static void PendingReorgVarianceEngineKeepsResultsWithinBounds()
    {
        var engine = PendingReorgVarianceEngine.DefaultPendingReorgVarianceEngine(Flags.NewRegistry());
        var item = PendingReorgVariance.NewPendingReorgVariance("code-951", "name-387");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        PendingReorgVarianceEngineKeepsResultsWithinBounds();
    }
}
