namespace Acme.Workforce.Orgdesign.Tests.CostModel.ModelSummary;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Orgdesign.CostModel.ModelSummary;

/// <summary>Checks of model summary code, run by the project's runner.</summary>
public static class BatchedCostModelBatchEngineChecks
{
    public static void BatchedCostModelBatchEngineKeepsResultsWithinBounds()
    {
        var engine = BatchedCostModelBatchEngine.DefaultBatchedCostModelBatchEngine(Flags.NewRegistry());
        var item = BatchedCostModelBatch.NewBatchedCostModelBatch("code-602", "title-675");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        BatchedCostModelBatchEngineKeepsResultsWithinBounds();
    }
}
