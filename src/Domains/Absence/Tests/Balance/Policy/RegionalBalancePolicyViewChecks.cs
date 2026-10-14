namespace Acme.Workforce.Absence.Tests.Balance.Policy;

using Acme.Workforce.Absence.Balance.Policy;

/// <summary>Checks of policy code, run by the project's runner.</summary>
public static class RegionalBalancePolicyViewChecks
{
    public static void RegionalBalancePolicyViewShowsTheStatusInCapitals()
    {
        var item = RegionalBalancePolicy.NewRegionalBalancePolicy("code-651", "title-480");
        var view = RegionalBalancePolicyView.ToRegionalBalancePolicyView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(RegionalBalancePolicyView.ToRegionalBalancePolicyViews(new List<RegionalBalancePolicy> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        RegionalBalancePolicyViewShowsTheStatusInCapitals();
    }
}
