namespace Acme.Workforce.Engagement.Tests.ActionPlan.RecordItem;

using Acme.Workforce.Engagement.ActionPlan.RecordItem;

/// <summary>Checks of record item code, run by the project's runner.</summary>
public static class ActionPlanRuleEngineChecks
{
    public static void ActionPlanRuleEngineKeepsResultsWithinBounds()
    {
        var engine = ActionPlanRuleEngine.DefaultActionPlanRuleEngine();
        var item = ActionPlanRule.NewActionPlanRule("code-961", "name-670");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        ActionPlanRuleEngineKeepsResultsWithinBounds();
    }
}
