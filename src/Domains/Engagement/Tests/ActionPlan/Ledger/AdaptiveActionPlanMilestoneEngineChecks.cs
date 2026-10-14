namespace Acme.Workforce.Engagement.Tests.ActionPlan.Ledger;

using Acme.Workforce.Engagement.ActionPlan.Ledger;

/// <summary>Checks of ledger code, run by the project's runner.</summary>
public static class AdaptiveActionPlanMilestoneEngineChecks
{
    public static void AdaptiveActionPlanMilestoneEngineKeepsResultsWithinBounds()
    {
        var engine = AdaptiveActionPlanMilestoneEngine.DefaultAdaptiveActionPlanMilestoneEngine();
        var item = AdaptiveActionPlanMilestone.NewAdaptiveActionPlanMilestone("code-18", "title-133");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        AdaptiveActionPlanMilestoneEngineKeepsResultsWithinBounds();
    }
}
