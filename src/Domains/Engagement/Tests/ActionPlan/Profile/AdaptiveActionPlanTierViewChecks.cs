namespace Acme.Workforce.Engagement.Tests.ActionPlan.Profile;

using Acme.Workforce.Engagement.ActionPlan.Profile;

/// <summary>Checks of profile code, run by the project's runner.</summary>
public static class AdaptiveActionPlanTierViewChecks
{
    public static void AdaptiveActionPlanTierViewShowsTheStatusInCapitals()
    {
        var item = AdaptiveActionPlanTier.NewAdaptiveActionPlanTier("code-216", "name-832");
        var view = AdaptiveActionPlanTierView.ToAdaptiveActionPlanTierView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(AdaptiveActionPlanTierView.ToAdaptiveActionPlanTierViews(new List<AdaptiveActionPlanTier> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        AdaptiveActionPlanTierViewShowsTheStatusInCapitals();
    }
}
