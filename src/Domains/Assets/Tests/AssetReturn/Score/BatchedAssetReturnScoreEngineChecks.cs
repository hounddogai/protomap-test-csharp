namespace Acme.Workforce.Assets.Tests.AssetReturn.Score;

using Acme.Workforce.Assets.AssetReturn.Score;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of score code, run by the project's runner.</summary>
public static class BatchedAssetReturnScoreEngineChecks
{
    public static void BatchedAssetReturnScoreEngineKeepsResultsWithinBounds()
    {
        var engine = BatchedAssetReturnScoreEngine.DefaultBatchedAssetReturnScoreEngine(Flags.NewRegistry());
        var item = BatchedAssetReturnScore.NewBatchedAssetReturnScore("code-402", "name-416");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        BatchedAssetReturnScoreEngineKeepsResultsWithinBounds();
    }
}
