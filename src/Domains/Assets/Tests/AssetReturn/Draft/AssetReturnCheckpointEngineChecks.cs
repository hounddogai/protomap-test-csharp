namespace Acme.Workforce.Assets.Tests.AssetReturn.Draft;

using Acme.Workforce.Assets.AssetReturn.Draft;

/// <summary>Checks of draft code, run by the project's runner.</summary>
public static class AssetReturnCheckpointEngineChecks
{
    public static void AssetReturnCheckpointEngineKeepsResultsWithinBounds()
    {
        var engine = AssetReturnCheckpointEngine.DefaultAssetReturnCheckpointEngine();
        var item = AssetReturnCheckpoint.NewAssetReturnCheckpoint("code-45", "name-320");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        AssetReturnCheckpointEngineKeepsResultsWithinBounds();
    }
}
