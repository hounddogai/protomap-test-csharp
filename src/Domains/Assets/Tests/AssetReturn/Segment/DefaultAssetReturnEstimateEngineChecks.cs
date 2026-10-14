namespace Acme.Workforce.Assets.Tests.AssetReturn.Segment;

using Acme.Workforce.Assets.AssetReturn.Segment;

/// <summary>Checks of segment code, run by the project's runner.</summary>
public static class DefaultAssetReturnEstimateEngineChecks
{
    public static void DefaultAssetReturnEstimateEngineKeepsResultsWithinBounds()
    {
        var engine = DefaultAssetReturnEstimateEngine.DefaultDefaultAssetReturnEstimateEngine();
        var item = DefaultAssetReturnEstimate.NewDefaultAssetReturnEstimate("code-600", "title-992");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        DefaultAssetReturnEstimateEngineKeepsResultsWithinBounds();
    }
}
