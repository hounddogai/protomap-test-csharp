namespace Acme.Workforce.Assets.Tests.AssetReturn.Budget;

using Acme.Workforce.Assets.AssetReturn.Budget;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of budget code, run by the project's runner.</summary>
public static class BatchedAssetReturnBudgetEngineChecks
{
    public static void BatchedAssetReturnBudgetEngineKeepsResultsWithinBounds()
    {
        var engine = BatchedAssetReturnBudgetEngine.DefaultBatchedAssetReturnBudgetEngine(Flags.NewRegistry());
        var item = BatchedAssetReturnBudget.NewBatchedAssetReturnBudget("code-282", "title-337");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        BatchedAssetReturnBudgetEngineKeepsResultsWithinBounds();
    }
}
