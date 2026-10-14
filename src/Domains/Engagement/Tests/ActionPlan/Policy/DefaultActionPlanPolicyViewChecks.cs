namespace Acme.Workforce.Engagement.Tests.ActionPlan.Policy;

using Acme.Workforce.Engagement.ActionPlan.Policy;

/// <summary>Checks of policy code, run by the project's runner.</summary>
public static class DefaultActionPlanPolicyViewChecks
{
    public static void DefaultActionPlanPolicyViewShowsTheStatusInCapitals()
    {
        var item = DefaultActionPlanPolicy.NewDefaultActionPlanPolicy("code-938", "label-25");
        var view = DefaultActionPlanPolicyView.ToDefaultActionPlanPolicyView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(DefaultActionPlanPolicyView.ToDefaultActionPlanPolicyViews(new List<DefaultActionPlanPolicy> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        DefaultActionPlanPolicyViewShowsTheStatusInCapitals();
    }
}
