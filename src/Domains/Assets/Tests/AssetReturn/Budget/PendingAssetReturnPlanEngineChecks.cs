namespace Acme.Workforce.Assets.Tests.AssetReturn.Budget;

using Acme.Workforce.Assets.AssetReturn.Budget;

/// <summary>Checks of budget code, run by the project's runner.</summary>
public static class PendingAssetReturnPlanEngineChecks
{
    public static void PendingAssetReturnPlanEngineKeepsResultsWithinBounds()
    {
        var engine = PendingAssetReturnPlanEngine.DefaultPendingAssetReturnPlanEngine();
        var item = PendingAssetReturnPlan.NewPendingAssetReturnPlan("code-531", "label-347");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        PendingAssetReturnPlanEngineKeepsResultsWithinBounds();
    }
}
