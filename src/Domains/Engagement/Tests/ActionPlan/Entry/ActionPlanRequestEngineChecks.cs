namespace Acme.Workforce.Engagement.Tests.ActionPlan.Entry;

using Acme.Workforce.Engagement.ActionPlan.Entry;

/// <summary>Checks of entry code, run by the project's runner.</summary>
public static class ActionPlanRequestEngineChecks
{
    public static void ActionPlanRequestEngineKeepsResultsWithinBounds()
    {
        var engine = ActionPlanRequestEngine.DefaultActionPlanRequestEngine();
        var item = ActionPlanRequest.NewActionPlanRequest("code-526", "name-993");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        ActionPlanRequestEngineKeepsResultsWithinBounds();
    }
}
