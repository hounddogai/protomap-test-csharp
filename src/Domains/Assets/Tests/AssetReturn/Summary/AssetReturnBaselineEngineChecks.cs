namespace Acme.Workforce.Assets.Tests.AssetReturn.Summary;

using Acme.Workforce.Assets.AssetReturn.Summary;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of summary code, run by the project's runner.</summary>
public static class AssetReturnBaselineEngineChecks
{
    public static void AssetReturnBaselineEngineKeepsResultsWithinBounds()
    {
        var engine = AssetReturnBaselineEngine.DefaultAssetReturnBaselineEngine(Flags.NewRegistry());
        var item = AssetReturnBaseline.NewAssetReturnBaseline("code-701", "title-243");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        AssetReturnBaselineEngineKeepsResultsWithinBounds();
    }
}
