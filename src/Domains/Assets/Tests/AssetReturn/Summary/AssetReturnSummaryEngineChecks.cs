namespace Acme.Workforce.Assets.Tests.AssetReturn.Summary;

using Acme.Workforce.Assets.AssetReturn.Summary;

/// <summary>Checks of summary code, run by the project's runner.</summary>
public static class AssetReturnSummaryEngineChecks
{
    public static void AssetReturnSummaryEngineKeepsResultsWithinBounds()
    {
        var engine = AssetReturnSummaryEngine.DefaultAssetReturnSummaryEngine();
        var item = AssetReturnSummary.NewAssetReturnSummary("code-98", "label-790");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        AssetReturnSummaryEngineKeepsResultsWithinBounds();
    }
}
