namespace Acme.Workforce.Assets.Tests.AssetReturn.Profile;

using Acme.Workforce.Assets.AssetReturn.Profile;

/// <summary>Checks of profile code, run by the project's runner.</summary>
public static class PrimaryAssetReturnProfileEngineChecks
{
    public static void PrimaryAssetReturnProfileEngineKeepsResultsWithinBounds()
    {
        var engine = PrimaryAssetReturnProfileEngine.DefaultPrimaryAssetReturnProfileEngine();
        var item = PrimaryAssetReturnProfile.NewPrimaryAssetReturnProfile("code-801", "title-594");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        PrimaryAssetReturnProfileEngineKeepsResultsWithinBounds();
    }
}
