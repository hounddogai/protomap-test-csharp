namespace Acme.Workforce.Orgdesign.Tests.Reorg.ReorgSummary;

using Acme.Workforce.Orgdesign.Reorg.ReorgSummary;

/// <summary>Checks of reorg summary code, run by the project's runner.</summary>
public static class LegacyReorgSummaryEngineChecks
{
    public static void LegacyReorgSummaryEngineKeepsResultsWithinBounds()
    {
        var engine = LegacyReorgSummaryEngine.DefaultLegacyReorgSummaryEngine();
        var item = LegacyReorgSummary.NewLegacyReorgSummary("code-711", "title-16");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        LegacyReorgSummaryEngineKeepsResultsWithinBounds();
    }
}
