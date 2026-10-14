namespace Acme.Workforce.Orgdesign.Tests.Headcount.Slot;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Orgdesign.Headcount.Slot;

/// <summary>Checks of slot code, run by the project's runner.</summary>
public static class TieredHeadcountEstimateEngineChecks
{
    public static void TieredHeadcountEstimateEngineKeepsResultsWithinBounds()
    {
        var engine = TieredHeadcountEstimateEngine.DefaultTieredHeadcountEstimateEngine(Flags.NewRegistry());
        var item = TieredHeadcountEstimate.NewTieredHeadcountEstimate("code-601", "name-701");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        TieredHeadcountEstimateEngineKeepsResultsWithinBounds();
    }
}
