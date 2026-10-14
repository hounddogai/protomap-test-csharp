namespace Acme.Workforce.Engagement.Tests.ActionPlan.Profile;

using Acme.Workforce.Engagement.ActionPlan.Profile;

/// <summary>Checks of profile code, run by the project's runner.</summary>
public static class AdaptiveActionPlanTierEngineChecks
{
    public static void AdaptiveActionPlanTierEngineKeepsResultsWithinBounds()
    {
        var engine = AdaptiveActionPlanTierEngine.DefaultAdaptiveActionPlanTierEngine();
        var item = AdaptiveActionPlanTier.NewAdaptiveActionPlanTier("code-216", "name-832");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        AdaptiveActionPlanTierEngineKeepsResultsWithinBounds();
    }
}
