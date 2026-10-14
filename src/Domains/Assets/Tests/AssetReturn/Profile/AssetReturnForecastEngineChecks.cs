namespace Acme.Workforce.Assets.Tests.AssetReturn.Profile;

using Acme.Workforce.Assets.AssetReturn.Profile;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of profile code, run by the project's runner.</summary>
public static class AssetReturnForecastEngineChecks
{
    public static void AssetReturnForecastEngineKeepsResultsWithinBounds()
    {
        var engine = AssetReturnForecastEngine.DefaultAssetReturnForecastEngine(Flags.NewRegistry());
        var item = AssetReturnForecast.NewAssetReturnForecast("code-795", "name-694");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        AssetReturnForecastEngineKeepsResultsWithinBounds();
    }
}
