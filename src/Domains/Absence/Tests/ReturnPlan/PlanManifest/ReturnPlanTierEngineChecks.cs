namespace Acme.Workforce.Absence.Tests.ReturnPlan.PlanManifest;

using Acme.Workforce.Absence.ReturnPlan.PlanManifest;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of plan manifest code, run by the project's runner.</summary>
public static class ReturnPlanTierEngineChecks
{
    public static void ReturnPlanTierEngineKeepsResultsWithinBounds()
    {
        var engine = ReturnPlanTierEngine.DefaultReturnPlanTierEngine(Flags.NewRegistry());
        var item = ReturnPlanTier.NewReturnPlanTier("code-807", "label-348");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        ReturnPlanTierEngineKeepsResultsWithinBounds();
    }
}
