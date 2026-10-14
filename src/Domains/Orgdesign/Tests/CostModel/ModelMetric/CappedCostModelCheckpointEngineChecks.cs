namespace Acme.Workforce.Orgdesign.Tests.CostModel.ModelMetric;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Orgdesign.CostModel.ModelMetric;

/// <summary>Checks of model metric code, run by the project's runner.</summary>
public static class CappedCostModelCheckpointEngineChecks
{
    public static void CappedCostModelCheckpointEngineKeepsResultsWithinBounds()
    {
        var engine = CappedCostModelCheckpointEngine.DefaultCappedCostModelCheckpointEngine(Flags.NewRegistry());
        var item = CappedCostModelCheckpoint.NewCappedCostModelCheckpoint("code-561", "label-223");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        CappedCostModelCheckpointEngineKeepsResultsWithinBounds();
    }
}
