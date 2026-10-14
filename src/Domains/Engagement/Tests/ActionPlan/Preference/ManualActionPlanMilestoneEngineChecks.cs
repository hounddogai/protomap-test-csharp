namespace Acme.Workforce.Engagement.Tests.ActionPlan.Preference;

using Acme.Workforce.Engagement.ActionPlan.Preference;

/// <summary>Checks of preference code, run by the project's runner.</summary>
public static class ManualActionPlanMilestoneEngineChecks
{
    public static void ManualActionPlanMilestoneEngineKeepsResultsWithinBounds()
    {
        var engine = ManualActionPlanMilestoneEngine.DefaultManualActionPlanMilestoneEngine();
        var item = ManualActionPlanMilestone.NewManualActionPlanMilestone("code-649", "label-745");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        ManualActionPlanMilestoneEngineKeepsResultsWithinBounds();
    }
}
