namespace Acme.Workforce.Talent.Tests.SuccessionPlan.Policy;

using Acme.Workforce.Talent.SuccessionPlan.Policy;

/// <summary>Checks of policy code, run by the project's runner.</summary>
public static class LegacySuccessionPlanPolicyViewChecks
{
    public static void LegacySuccessionPlanPolicyViewShowsTheStatusInCapitals()
    {
        var item = LegacySuccessionPlanPolicy.NewLegacySuccessionPlanPolicy("code-73", "label-872");
        var view = LegacySuccessionPlanPolicyView.ToLegacySuccessionPlanPolicyView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(LegacySuccessionPlanPolicyView.ToLegacySuccessionPlanPolicyViews(new List<LegacySuccessionPlanPolicy> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        LegacySuccessionPlanPolicyViewShowsTheStatusInCapitals();
    }
}
