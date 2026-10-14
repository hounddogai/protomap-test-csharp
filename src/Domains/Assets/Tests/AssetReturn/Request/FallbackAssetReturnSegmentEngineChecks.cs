namespace Acme.Workforce.Assets.Tests.AssetReturn.Request;

using Acme.Workforce.Assets.AssetReturn.Request;

/// <summary>Checks of request code, run by the project's runner.</summary>
public static class FallbackAssetReturnSegmentEngineChecks
{
    public static void FallbackAssetReturnSegmentEngineKeepsResultsWithinBounds()
    {
        var engine = FallbackAssetReturnSegmentEngine.DefaultFallbackAssetReturnSegmentEngine();
        var item = FallbackAssetReturnSegment.NewFallbackAssetReturnSegment("code-403", "title-868");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        FallbackAssetReturnSegmentEngineKeepsResultsWithinBounds();
    }
}
