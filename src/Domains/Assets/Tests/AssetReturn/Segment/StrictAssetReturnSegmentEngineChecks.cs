namespace Acme.Workforce.Assets.Tests.AssetReturn.Segment;

using Acme.Workforce.Assets.AssetReturn.Segment;

/// <summary>Checks of segment code, run by the project's runner.</summary>
public static class StrictAssetReturnSegmentEngineChecks
{
    public static void StrictAssetReturnSegmentEngineKeepsResultsWithinBounds()
    {
        var engine = StrictAssetReturnSegmentEngine.DefaultStrictAssetReturnSegmentEngine();
        var item = StrictAssetReturnSegment.NewStrictAssetReturnSegment("code-122", "label-577");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        StrictAssetReturnSegmentEngineKeepsResultsWithinBounds();
    }
}
