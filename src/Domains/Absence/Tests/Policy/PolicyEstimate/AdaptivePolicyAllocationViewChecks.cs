namespace Acme.Workforce.Absence.Tests.Policy.PolicyEstimate;

using Acme.Workforce.Absence.Policy.PolicyEstimate;

/// <summary>Checks of policy estimate code, run by the project's runner.</summary>
public static class AdaptivePolicyAllocationViewChecks
{
    public static void AdaptivePolicyAllocationViewShowsTheStatusInCapitals()
    {
        var item = AdaptivePolicyAllocation.NewAdaptivePolicyAllocation("code-759", "label-649");
        var view = AdaptivePolicyAllocationView.ToAdaptivePolicyAllocationView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(AdaptivePolicyAllocationView.ToAdaptivePolicyAllocationViews(new List<AdaptivePolicyAllocation> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        AdaptivePolicyAllocationViewShowsTheStatusInCapitals();
    }
}
