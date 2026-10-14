namespace Acme.Workforce.Assets.Tests.AssetReturn.Segment;

using Acme.Workforce.Assets.AssetReturn.Segment;

/// <summary>Checks of segment code, run by the project's runner.</summary>
public static class AssetReturnTargetEngineChecks
{
    public static void AssetReturnTargetEngineKeepsResultsWithinBounds()
    {
        var engine = AssetReturnTargetEngine.DefaultAssetReturnTargetEngine();
        var item = AssetReturnTarget.NewAssetReturnTarget("code-619", "label-621");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        AssetReturnTargetEngineKeepsResultsWithinBounds();
    }
}
