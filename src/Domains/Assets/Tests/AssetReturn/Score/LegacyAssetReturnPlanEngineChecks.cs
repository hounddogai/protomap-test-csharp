namespace Acme.Workforce.Assets.Tests.AssetReturn.Score;

using Acme.Workforce.Assets.AssetReturn.Score;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of score code, run by the project's runner.</summary>
public static class LegacyAssetReturnPlanEngineChecks
{
    public static void LegacyAssetReturnPlanEngineKeepsResultsWithinBounds()
    {
        var engine = LegacyAssetReturnPlanEngine.DefaultLegacyAssetReturnPlanEngine(Flags.NewRegistry());
        var item = LegacyAssetReturnPlan.NewLegacyAssetReturnPlan("code-983", "title-160");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        LegacyAssetReturnPlanEngineKeepsResultsWithinBounds();
    }
}
