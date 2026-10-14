namespace Acme.Workforce.Absence.Tests.ReturnPlan.PlanAudit;

using Acme.Workforce.Absence.ReturnPlan.PlanAudit;

/// <summary>Checks of plan audit code, run by the project's runner.</summary>
public static class WeightedReturnPlanOverrideEngineChecks
{
    public static void WeightedReturnPlanOverrideEngineKeepsResultsWithinBounds()
    {
        var engine = WeightedReturnPlanOverrideEngine.DefaultWeightedReturnPlanOverrideEngine();
        var item = WeightedReturnPlanOverride.NewWeightedReturnPlanOverride("code-322", "label-710");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        WeightedReturnPlanOverrideEngineKeepsResultsWithinBounds();
    }
}
