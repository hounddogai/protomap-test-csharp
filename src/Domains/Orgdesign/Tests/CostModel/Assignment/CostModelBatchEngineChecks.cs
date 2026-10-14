namespace Acme.Workforce.Orgdesign.Tests.CostModel.Assignment;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Orgdesign.CostModel.Assignment;

/// <summary>Checks of assignment code, run by the project's runner.</summary>
public static class CostModelBatchEngineChecks
{
    public static void CostModelBatchEngineKeepsResultsWithinBounds()
    {
        var engine = CostModelBatchEngine.DefaultCostModelBatchEngine(Flags.NewRegistry());
        var item = CostModelBatch.NewCostModelBatch("code-697", "name-888");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        CostModelBatchEngineKeepsResultsWithinBounds();
    }
}
