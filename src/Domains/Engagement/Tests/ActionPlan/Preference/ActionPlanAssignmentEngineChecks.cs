namespace Acme.Workforce.Engagement.Tests.ActionPlan.Preference;

using Acme.Workforce.Engagement.ActionPlan.Preference;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of preference code, run by the project's runner.</summary>
public static class ActionPlanAssignmentEngineChecks
{
    public static void ActionPlanAssignmentEngineKeepsResultsWithinBounds()
    {
        var engine = ActionPlanAssignmentEngine.DefaultActionPlanAssignmentEngine(Flags.NewRegistry());
        var item = ActionPlanAssignment.NewActionPlanAssignment("code-73", "title-697");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        ActionPlanAssignmentEngineKeepsResultsWithinBounds();
    }
}
