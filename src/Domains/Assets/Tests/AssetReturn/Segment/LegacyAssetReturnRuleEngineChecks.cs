namespace Acme.Workforce.Assets.Tests.AssetReturn.Segment;

using Acme.Workforce.Assets.AssetReturn.Segment;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of segment code, run by the project's runner.</summary>
public static class LegacyAssetReturnRuleEngineChecks
{
    public static void LegacyAssetReturnRuleEngineKeepsResultsWithinBounds()
    {
        var engine = LegacyAssetReturnRuleEngine.DefaultLegacyAssetReturnRuleEngine(Flags.NewRegistry());
        var item = LegacyAssetReturnRule.NewLegacyAssetReturnRule("code-354", "name-288");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        LegacyAssetReturnRuleEngineKeepsResultsWithinBounds();
    }
}
