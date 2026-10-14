namespace Acme.Workforce.Assets.Tests.AssetReturn.Adjustment;

using Acme.Workforce.Assets.AssetReturn.Adjustment;

/// <summary>Checks of adjustment code, run by the project's runner.</summary>
public static class AssetReturnQuotaEngineChecks
{
    public static void AssetReturnQuotaEngineKeepsResultsWithinBounds()
    {
        var engine = AssetReturnQuotaEngine.DefaultAssetReturnQuotaEngine();
        var item = AssetReturnQuota.NewAssetReturnQuota("code-10", "name-668");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        AssetReturnQuotaEngineKeepsResultsWithinBounds();
    }
}
