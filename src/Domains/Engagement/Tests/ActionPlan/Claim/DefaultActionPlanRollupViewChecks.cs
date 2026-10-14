namespace Acme.Workforce.Engagement.Tests.ActionPlan.Claim;

using Acme.Workforce.Engagement.ActionPlan.Claim;

/// <summary>Checks of claim code, run by the project's runner.</summary>
public static class DefaultActionPlanRollupViewChecks
{
    public static void DefaultActionPlanRollupViewShowsTheStatusInCapitals()
    {
        var item = DefaultActionPlanRollup.NewDefaultActionPlanRollup("code-883", "label-183");
        var view = DefaultActionPlanRollupView.ToDefaultActionPlanRollupView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(DefaultActionPlanRollupView.ToDefaultActionPlanRollupViews(new List<DefaultActionPlanRollup> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        DefaultActionPlanRollupViewShowsTheStatusInCapitals();
    }
}
