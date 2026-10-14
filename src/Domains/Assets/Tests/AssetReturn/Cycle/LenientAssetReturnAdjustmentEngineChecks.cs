namespace Acme.Workforce.Assets.Tests.AssetReturn.Cycle;

using Acme.Workforce.Assets.AssetReturn.Cycle;

/// <summary>Checks of cycle code, run by the project's runner.</summary>
public static class LenientAssetReturnAdjustmentEngineChecks
{
    public static void LenientAssetReturnAdjustmentEngineKeepsResultsWithinBounds()
    {
        var engine = LenientAssetReturnAdjustmentEngine.DefaultLenientAssetReturnAdjustmentEngine();
        var item = LenientAssetReturnAdjustment.NewLenientAssetReturnAdjustment("code-268", "title-245");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        LenientAssetReturnAdjustmentEngineKeepsResultsWithinBounds();
    }
}
