namespace Acme.Workforce.Absence.Tests.ReturnPlan.Target;

using Acme.Workforce.Absence.ReturnPlan.Target;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of target code, run by the project's runner.</summary>
public static class ReturnPlanTargetEngineChecks
{
    public static void ReturnPlanTargetEngineKeepsResultsWithinBounds()
    {
        var engine = ReturnPlanTargetEngine.DefaultReturnPlanTargetEngine(Flags.NewRegistry());
        var item = ReturnPlanTarget.NewReturnPlanTarget("code-723", "label-310");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        ReturnPlanTargetEngineKeepsResultsWithinBounds();
    }
}
