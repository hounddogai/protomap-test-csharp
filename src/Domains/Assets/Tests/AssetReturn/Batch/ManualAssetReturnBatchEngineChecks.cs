namespace Acme.Workforce.Assets.Tests.AssetReturn.Batch;

using Acme.Workforce.Assets.AssetReturn.Batch;

/// <summary>Checks of batch code, run by the project's runner.</summary>
public static class ManualAssetReturnBatchEngineChecks
{
    public static void ManualAssetReturnBatchEngineKeepsResultsWithinBounds()
    {
        var engine = ManualAssetReturnBatchEngine.DefaultManualAssetReturnBatchEngine();
        var item = ManualAssetReturnBatch.NewManualAssetReturnBatch("code-959", "label-370");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        ManualAssetReturnBatchEngineKeepsResultsWithinBounds();
    }
}
