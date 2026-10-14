namespace Acme.Workforce.Orgdesign.Tests.CostModel.AdaptiveModelPolicy;

using Acme.Workforce.Orgdesign.CostModel.AdaptiveModelPolicy;

/// <summary>Checks of adaptive model policy code, run by the project's runner.</summary>
public static class PendingCostModelPolicyViewChecks
{
    public static void PendingCostModelPolicyViewShowsTheStatusInCapitals()
    {
        var item = PendingCostModelPolicy.NewPendingCostModelPolicy("code-411", "label-378");
        var view = PendingCostModelPolicyView.ToPendingCostModelPolicyView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(PendingCostModelPolicyView.ToPendingCostModelPolicyViews(new List<PendingCostModelPolicy> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        PendingCostModelPolicyViewShowsTheStatusInCapitals();
    }
}
