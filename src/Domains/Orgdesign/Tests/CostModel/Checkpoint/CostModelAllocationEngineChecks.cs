namespace Acme.Workforce.Orgdesign.Tests.CostModel.Checkpoint;

using Acme.Workforce.Orgdesign.CostModel.Checkpoint;

/// <summary>Checks of checkpoint code, run by the project's runner.</summary>
public static class CostModelAllocationEngineChecks
{
    public static void CostModelAllocationEngineKeepsResultsWithinBounds()
    {
        var engine = CostModelAllocationEngine.DefaultCostModelAllocationEngine();
        var item = CostModelAllocation.NewCostModelAllocation("code-548", "label-346");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        CostModelAllocationEngineKeepsResultsWithinBounds();
    }
}
