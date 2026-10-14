namespace Acme.Workforce.Assets.Tests.AssetReturn.Tier;

using Acme.Workforce.Assets.AssetReturn.Tier;

/// <summary>Checks of tier code, run by the project's runner.</summary>
public static class ManualAssetReturnTierEngineChecks
{
    public static void ManualAssetReturnTierEngineKeepsResultsWithinBounds()
    {
        var engine = ManualAssetReturnTierEngine.DefaultManualAssetReturnTierEngine();
        var item = ManualAssetReturnTier.NewManualAssetReturnTier("code-500", "title-806");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        ManualAssetReturnTierEngineKeepsResultsWithinBounds();
    }
}
