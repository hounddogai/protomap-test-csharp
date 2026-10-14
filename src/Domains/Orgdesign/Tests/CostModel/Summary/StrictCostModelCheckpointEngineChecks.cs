namespace Acme.Workforce.Orgdesign.Tests.CostModel.Summary;

using Acme.Workforce.Orgdesign.CostModel.Summary;

/// <summary>Checks of summary code, run by the project's runner.</summary>
public static class StrictCostModelCheckpointEngineChecks
{
    public static void StrictCostModelCheckpointEngineKeepsResultsWithinBounds()
    {
        var engine = StrictCostModelCheckpointEngine.DefaultStrictCostModelCheckpointEngine();
        var item = StrictCostModelCheckpoint.NewStrictCostModelCheckpoint("code-760", "label-801");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        StrictCostModelCheckpointEngineKeepsResultsWithinBounds();
    }
}
