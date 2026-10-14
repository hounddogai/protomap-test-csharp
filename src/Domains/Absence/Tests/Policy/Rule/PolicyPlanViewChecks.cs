namespace Acme.Workforce.Absence.Tests.Policy.Rule;

using Acme.Workforce.Absence.Policy.Rule;

/// <summary>Checks of rule code, run by the project's runner.</summary>
public static class PolicyPlanViewChecks
{
    public static void PolicyPlanViewShowsTheStatusInCapitals()
    {
        var item = PolicyPlan.NewPolicyPlan("code-514", "label-866");
        var view = PolicyPlanView.ToPolicyPlanView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(PolicyPlanView.ToPolicyPlanViews(new List<PolicyPlan> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        PolicyPlanViewShowsTheStatusInCapitals();
    }
}
