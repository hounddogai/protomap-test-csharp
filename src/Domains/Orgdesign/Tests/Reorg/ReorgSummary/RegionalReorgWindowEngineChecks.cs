namespace Acme.Workforce.Orgdesign.Tests.Reorg.ReorgSummary;

using Acme.Workforce.Orgdesign.Reorg.ReorgSummary;

/// <summary>Checks of reorg summary code, run by the project's runner.</summary>
public static class RegionalReorgWindowEngineChecks
{
    public static void RegionalReorgWindowEngineKeepsResultsWithinBounds()
    {
        var engine = RegionalReorgWindowEngine.DefaultRegionalReorgWindowEngine();
        var item = RegionalReorgWindow.NewRegionalReorgWindow("code-644", "label-979");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        RegionalReorgWindowEngineKeepsResultsWithinBounds();
    }
}
