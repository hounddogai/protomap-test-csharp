namespace Acme.Workforce.Orgdesign.Tests.CostModel.ModelEntry;

using Acme.Workforce.Orgdesign.CostModel.ModelEntry;

/// <summary>Checks of model entry code, run by the project's runner.</summary>
public static class TieredCostModelEntryEngineChecks
{
    public static void TieredCostModelEntryEngineKeepsResultsWithinBounds()
    {
        var engine = TieredCostModelEntryEngine.DefaultTieredCostModelEntryEngine();
        var item = TieredCostModelEntry.NewTieredCostModelEntry("code-336", "name-76");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        TieredCostModelEntryEngineKeepsResultsWithinBounds();
    }
}
