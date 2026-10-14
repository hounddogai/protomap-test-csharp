namespace Acme.Workforce.Assets.Tests.AssetReturn.Profile;

using Acme.Workforce.Assets.AssetReturn.Profile;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of profile code, run by the project's runner.</summary>
public static class StrictAssetReturnTierEngineChecks
{
    public static void StrictAssetReturnTierEngineKeepsResultsWithinBounds()
    {
        var engine = StrictAssetReturnTierEngine.DefaultStrictAssetReturnTierEngine(Flags.NewRegistry());
        var item = StrictAssetReturnTier.NewStrictAssetReturnTier("code-492", "label-186");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        StrictAssetReturnTierEngineKeepsResultsWithinBounds();
    }
}
