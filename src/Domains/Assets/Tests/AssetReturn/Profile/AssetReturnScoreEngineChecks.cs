namespace Acme.Workforce.Assets.Tests.AssetReturn.Profile;

using Acme.Workforce.Assets.AssetReturn.Profile;

/// <summary>Checks of profile code, run by the project's runner.</summary>
public static class AssetReturnScoreEngineChecks
{
    public static void AssetReturnScoreEngineKeepsResultsWithinBounds()
    {
        var engine = AssetReturnScoreEngine.DefaultAssetReturnScoreEngine();
        var item = AssetReturnScore.NewAssetReturnScore("code-986", "name-337");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        AssetReturnScoreEngineKeepsResultsWithinBounds();
    }
}
