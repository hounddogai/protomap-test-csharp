namespace Acme.Workforce.Assets.Tests.AssetReturn.Adjustment;

using Acme.Workforce.Assets.AssetReturn.Adjustment;

/// <summary>Checks of adjustment code, run by the project's runner.</summary>
public static class AssetReturnAdjustmentEngineChecks
{
    public static void AssetReturnAdjustmentEngineKeepsResultsWithinBounds()
    {
        var engine = AssetReturnAdjustmentEngine.DefaultAssetReturnAdjustmentEngine();
        var item = AssetReturnAdjustment.NewAssetReturnAdjustment("code-775", "title-240");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        AssetReturnAdjustmentEngineKeepsResultsWithinBounds();
    }
}
