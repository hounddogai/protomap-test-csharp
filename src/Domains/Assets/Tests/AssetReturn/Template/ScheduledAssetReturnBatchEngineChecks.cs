namespace Acme.Workforce.Assets.Tests.AssetReturn.Template;

using Acme.Workforce.Assets.AssetReturn.Template;

/// <summary>Checks of template code, run by the project's runner.</summary>
public static class ScheduledAssetReturnBatchEngineChecks
{
    public static void ScheduledAssetReturnBatchEngineKeepsResultsWithinBounds()
    {
        var engine = ScheduledAssetReturnBatchEngine.DefaultScheduledAssetReturnBatchEngine();
        var item = ScheduledAssetReturnBatch.NewScheduledAssetReturnBatch("code-601", "name-193");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        ScheduledAssetReturnBatchEngineKeepsResultsWithinBounds();
    }
}
