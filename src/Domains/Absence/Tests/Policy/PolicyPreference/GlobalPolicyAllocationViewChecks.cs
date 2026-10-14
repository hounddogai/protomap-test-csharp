namespace Acme.Workforce.Absence.Tests.Policy.PolicyPreference;

using Acme.Workforce.Absence.Policy.PolicyPreference;

/// <summary>Checks of policy preference code, run by the project's runner.</summary>
public static class GlobalPolicyAllocationViewChecks
{
    public static void GlobalPolicyAllocationViewShowsTheStatusInCapitals()
    {
        var item = GlobalPolicyAllocation.NewGlobalPolicyAllocation("code-497", "name-777");
        var view = GlobalPolicyAllocationView.ToGlobalPolicyAllocationView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(GlobalPolicyAllocationView.ToGlobalPolicyAllocationViews(new List<GlobalPolicyAllocation> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        GlobalPolicyAllocationViewShowsTheStatusInCapitals();
    }
}
