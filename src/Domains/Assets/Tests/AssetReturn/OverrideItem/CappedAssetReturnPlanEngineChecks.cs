namespace Acme.Workforce.Assets.Tests.AssetReturn.OverrideItem;

using Acme.Workforce.Assets.AssetReturn.OverrideItem;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of override item code, run by the project's runner.</summary>
public static class CappedAssetReturnPlanEngineChecks
{
    public static void CappedAssetReturnPlanEngineKeepsResultsWithinBounds()
    {
        var engine = CappedAssetReturnPlanEngine.DefaultCappedAssetReturnPlanEngine(Flags.NewRegistry());
        var item = CappedAssetReturnPlan.NewCappedAssetReturnPlan("code-933", "name-463");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        CappedAssetReturnPlanEngineKeepsResultsWithinBounds();
    }
}
