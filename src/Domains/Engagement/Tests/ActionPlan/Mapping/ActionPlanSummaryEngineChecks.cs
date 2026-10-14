namespace Acme.Workforce.Engagement.Tests.ActionPlan.Mapping;

using Acme.Workforce.Engagement.ActionPlan.Mapping;

/// <summary>Checks of mapping code, run by the project's runner.</summary>
public static class ActionPlanSummaryEngineChecks
{
    public static void ActionPlanSummaryEngineKeepsResultsWithinBounds()
    {
        var engine = ActionPlanSummaryEngine.DefaultActionPlanSummaryEngine();
        var item = ActionPlanSummary.NewActionPlanSummary("code-506", "name-294");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        ActionPlanSummaryEngineKeepsResultsWithinBounds();
    }
}
