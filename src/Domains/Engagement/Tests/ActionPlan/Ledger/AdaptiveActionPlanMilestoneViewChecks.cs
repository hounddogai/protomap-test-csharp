namespace Acme.Workforce.Engagement.Tests.ActionPlan.Ledger;

using Acme.Workforce.Engagement.ActionPlan.Ledger;

/// <summary>Checks of ledger code, run by the project's runner.</summary>
public static class AdaptiveActionPlanMilestoneViewChecks
{
    public static void AdaptiveActionPlanMilestoneViewShowsTheStatusInCapitals()
    {
        var item = AdaptiveActionPlanMilestone.NewAdaptiveActionPlanMilestone("code-18", "title-133");
        var view = AdaptiveActionPlanMilestoneView.ToAdaptiveActionPlanMilestoneView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(AdaptiveActionPlanMilestoneView.ToAdaptiveActionPlanMilestoneViews(new List<AdaptiveActionPlanMilestone> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        AdaptiveActionPlanMilestoneViewShowsTheStatusInCapitals();
    }
}
