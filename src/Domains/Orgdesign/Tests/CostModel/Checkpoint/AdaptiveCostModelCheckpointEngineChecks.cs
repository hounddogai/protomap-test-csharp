namespace Acme.Workforce.Orgdesign.Tests.CostModel.Checkpoint;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Orgdesign.CostModel.Checkpoint;

/// <summary>Checks of checkpoint code, run by the project's runner.</summary>
public static class AdaptiveCostModelCheckpointEngineChecks
{
    public static void AdaptiveCostModelCheckpointEngineKeepsResultsWithinBounds()
    {
        var engine = AdaptiveCostModelCheckpointEngine.DefaultAdaptiveCostModelCheckpointEngine(Flags.NewRegistry());
        var item = AdaptiveCostModelCheckpoint.NewAdaptiveCostModelCheckpoint("code-80", "name-322");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        AdaptiveCostModelCheckpointEngineKeepsResultsWithinBounds();
    }
}
