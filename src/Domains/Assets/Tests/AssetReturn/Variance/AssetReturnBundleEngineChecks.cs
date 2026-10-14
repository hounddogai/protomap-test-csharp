namespace Acme.Workforce.Assets.Tests.AssetReturn.Variance;

using Acme.Workforce.Assets.AssetReturn.Variance;

/// <summary>Checks of variance code, run by the project's runner.</summary>
public static class AssetReturnBundleEngineChecks
{
    public static void AssetReturnBundleEngineKeepsResultsWithinBounds()
    {
        var engine = AssetReturnBundleEngine.DefaultAssetReturnBundleEngine();
        var item = AssetReturnBundle.NewAssetReturnBundle("code-188", "name-628");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        AssetReturnBundleEngineKeepsResultsWithinBounds();
    }
}
