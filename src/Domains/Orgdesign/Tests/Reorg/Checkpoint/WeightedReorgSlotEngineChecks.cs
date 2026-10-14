namespace Acme.Workforce.Orgdesign.Tests.Reorg.Checkpoint;

using Acme.Workforce.Orgdesign.Reorg.Checkpoint;

/// <summary>Checks of checkpoint code, run by the project's runner.</summary>
public static class WeightedReorgSlotEngineChecks
{
    public static void WeightedReorgSlotEngineKeepsResultsWithinBounds()
    {
        var engine = WeightedReorgSlotEngine.DefaultWeightedReorgSlotEngine();
        var item = WeightedReorgSlot.NewWeightedReorgSlot("code-453", "label-659");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        WeightedReorgSlotEngineKeepsResultsWithinBounds();
    }
}
