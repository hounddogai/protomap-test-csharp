namespace Acme.Workforce.Orgdesign.Tests.CostModel.Score;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Orgdesign.CostModel.Score;

/// <summary>Checks of score code, run by the project's runner.</summary>
public static class CostModelCheckpointEngineChecks
{
    public static void CostModelCheckpointEngineKeepsResultsWithinBounds()
    {
        var engine = CostModelCheckpointEngine.DefaultCostModelCheckpointEngine(Flags.NewRegistry());
        var item = CostModelCheckpoint.NewCostModelCheckpoint("code-308", "name-322");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        CostModelCheckpointEngineKeepsResultsWithinBounds();
    }
}
