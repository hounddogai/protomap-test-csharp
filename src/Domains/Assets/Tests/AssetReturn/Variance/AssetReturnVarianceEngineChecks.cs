namespace Acme.Workforce.Assets.Tests.AssetReturn.Variance;

using Acme.Workforce.Assets.AssetReturn.Variance;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of variance code, run by the project's runner.</summary>
public static class AssetReturnVarianceEngineChecks
{
    public static void AssetReturnVarianceEngineKeepsResultsWithinBounds()
    {
        var engine = AssetReturnVarianceEngine.DefaultAssetReturnVarianceEngine(Flags.NewRegistry());
        var item = AssetReturnVariance.NewAssetReturnVariance("code-897", "name-784");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        AssetReturnVarianceEngineKeepsResultsWithinBounds();
    }
}
