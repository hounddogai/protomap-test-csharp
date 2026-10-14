namespace Acme.Workforce.Assets.Tests.AssetReturn.OverrideItem;

using Acme.Workforce.Assets.AssetReturn.OverrideItem;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of override item code, run by the project's runner.</summary>
public static class AssetReturnTierEngineChecks
{
    public static void AssetReturnTierEngineKeepsResultsWithinBounds()
    {
        var engine = AssetReturnTierEngine.DefaultAssetReturnTierEngine(Flags.NewRegistry());
        var item = AssetReturnTier.NewAssetReturnTier("code-151", "label-997");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        AssetReturnTierEngineKeepsResultsWithinBounds();
    }
}
