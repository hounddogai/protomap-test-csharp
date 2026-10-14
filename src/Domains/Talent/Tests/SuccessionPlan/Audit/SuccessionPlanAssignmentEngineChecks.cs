namespace Acme.Workforce.Talent.Tests.SuccessionPlan.Audit;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Talent.SuccessionPlan.Audit;

/// <summary>Checks of audit code, run by the project's runner.</summary>
public static class SuccessionPlanAssignmentEngineChecks
{
    public static void SuccessionPlanAssignmentEngineKeepsResultsWithinBounds()
    {
        var engine = SuccessionPlanAssignmentEngine.DefaultSuccessionPlanAssignmentEngine(Flags.NewRegistry());
        var item = SuccessionPlanAssignment.NewSuccessionPlanAssignment("code-428", "label-144");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        SuccessionPlanAssignmentEngineKeepsResultsWithinBounds();
    }
}
