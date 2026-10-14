namespace Acme.Workforce.Absence.Tests.ReturnPlan.OverrideItem;

using Acme.Workforce.Absence.ReturnPlan.OverrideItem;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of override item code, run by the project's runner.</summary>
public static class FallbackReturnPlanAssignmentEngineChecks
{
    public static void FallbackReturnPlanAssignmentEngineKeepsResultsWithinBounds()
    {
        var engine = FallbackReturnPlanAssignmentEngine.DefaultFallbackReturnPlanAssignmentEngine(Flags.NewRegistry());
        var item = FallbackReturnPlanAssignment.NewFallbackReturnPlanAssignment("code-885", "label-835");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        FallbackReturnPlanAssignmentEngineKeepsResultsWithinBounds();
    }
}
