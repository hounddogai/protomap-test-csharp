namespace Acme.Workforce.Assets.Tests.AssetReturn.Estimate;

using Acme.Workforce.Assets.AssetReturn.Estimate;

/// <summary>Checks of estimate code, run by the project's runner.</summary>
public static class WeightedAssetReturnForecastEngineChecks
{
    public static void WeightedAssetReturnForecastEngineKeepsResultsWithinBounds()
    {
        var engine = WeightedAssetReturnForecastEngine.DefaultWeightedAssetReturnForecastEngine();
        var item = WeightedAssetReturnForecast.NewWeightedAssetReturnForecast("code-30", "name-731");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        WeightedAssetReturnForecastEngineKeepsResultsWithinBounds();
    }
}
