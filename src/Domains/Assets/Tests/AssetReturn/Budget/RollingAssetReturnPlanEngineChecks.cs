namespace Acme.Workforce.Assets.Tests.AssetReturn.Budget;

using Acme.Workforce.Assets.AssetReturn.Budget;

/// <summary>Checks of budget code, run by the project's runner.</summary>
public static class RollingAssetReturnPlanEngineChecks
{
    public static void RollingAssetReturnPlanEngineKeepsResultsWithinBounds()
    {
        var engine = RollingAssetReturnPlanEngine.DefaultRollingAssetReturnPlanEngine();
        var item = RollingAssetReturnPlan.NewRollingAssetReturnPlan("code-697", "label-148");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        RollingAssetReturnPlanEngineKeepsResultsWithinBounds();
    }
}
