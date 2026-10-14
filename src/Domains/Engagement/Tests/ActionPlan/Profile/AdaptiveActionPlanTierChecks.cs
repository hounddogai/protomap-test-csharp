namespace Acme.Workforce.Engagement.Tests.ActionPlan.Profile;

using Acme.Workforce.Engagement.ActionPlan.Profile;

/// <summary>Checks of profile code, run by the project's runner.</summary>
public static class AdaptiveActionPlanTierChecks
{
    public static void AdaptiveActionPlanTierStartsInIdle()
    {
        var item = AdaptiveActionPlanTier.NewAdaptiveActionPlanTier("code-216", "name-832");
        Check.Equal(item.Status, AdaptiveActionPlanTierStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void AdaptiveActionPlanTierMovesToWarming()
    {
        var item = AdaptiveActionPlanTier.NewAdaptiveActionPlanTier("code-216", "name-832");
        try
        {
            item.MoveTo(AdaptiveActionPlanTierStatus.Warming);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, AdaptiveActionPlanTierStatus.Warming, "status");
    }

    public static void AdaptiveActionPlanTierRejectsMovesFromStopped()
    {
        var item = AdaptiveActionPlanTier.NewAdaptiveActionPlanTier("code-216", "name-832");
        item.Status = AdaptiveActionPlanTierStatus.Stopped;
        Check.Fails(() => item.MoveTo(AdaptiveActionPlanTierStatus.Idle), "move");
    }

    public static void RunAll()
    {
        AdaptiveActionPlanTierStartsInIdle();
        AdaptiveActionPlanTierMovesToWarming();
        AdaptiveActionPlanTierRejectsMovesFromStopped();
    }
}
