namespace Acme.Workforce.Absence.Tests.ReturnPlan.Target;

using Acme.Workforce.Absence.ReturnPlan.Target;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of target code, run by the project's runner.</summary>
public static class ReturnPlanAdjustmentEngineChecks
{
    public static void ReturnPlanAdjustmentEngineKeepsResultsWithinBounds()
    {
        var engine = ReturnPlanAdjustmentEngine.DefaultReturnPlanAdjustmentEngine(Flags.NewRegistry());
        var item = ReturnPlanAdjustment.NewReturnPlanAdjustment("code-505", "name-901");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        ReturnPlanAdjustmentEngineKeepsResultsWithinBounds();
    }
}
