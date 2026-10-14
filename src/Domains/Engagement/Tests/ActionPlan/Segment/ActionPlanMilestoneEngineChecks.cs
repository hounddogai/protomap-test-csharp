namespace Acme.Workforce.Engagement.Tests.ActionPlan.Segment;

using Acme.Workforce.Engagement.ActionPlan.Segment;

/// <summary>Checks of segment code, run by the project's runner.</summary>
public static class ActionPlanMilestoneEngineChecks
{
    public static void ActionPlanMilestoneEngineKeepsResultsWithinBounds()
    {
        var engine = ActionPlanMilestoneEngine.DefaultActionPlanMilestoneEngine();
        var item = ActionPlanMilestone.NewActionPlanMilestone("code-186", "label-774");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        ActionPlanMilestoneEngineKeepsResultsWithinBounds();
    }
}
