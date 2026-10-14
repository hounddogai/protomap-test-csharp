namespace Acme.Workforce.Assets.Tests.AssetReturn.Estimate;

using Acme.Workforce.Assets.AssetReturn.Estimate;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of estimate code, run by the project's runner.</summary>
public static class AssetReturnEstimateEngineChecks
{
    public static void AssetReturnEstimateEngineKeepsResultsWithinBounds()
    {
        var engine = AssetReturnEstimateEngine.DefaultAssetReturnEstimateEngine(Flags.NewRegistry());
        var item = AssetReturnEstimate.NewAssetReturnEstimate("code-296", "title-603");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        AssetReturnEstimateEngineKeepsResultsWithinBounds();
    }
}
