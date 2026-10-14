namespace Acme.Workforce.Assets.Tests.AssetReturn.Quota;

using Acme.Workforce.Assets.AssetReturn.Quota;

/// <summary>Checks of quota code, run by the project's runner.</summary>
public static class SeasonalAssetReturnQuotaEngineChecks
{
    public static void SeasonalAssetReturnQuotaEngineKeepsResultsWithinBounds()
    {
        var engine = SeasonalAssetReturnQuotaEngine.DefaultSeasonalAssetReturnQuotaEngine();
        var item = SeasonalAssetReturnQuota.NewSeasonalAssetReturnQuota("code-96", "title-556");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        SeasonalAssetReturnQuotaEngineKeepsResultsWithinBounds();
    }
}
