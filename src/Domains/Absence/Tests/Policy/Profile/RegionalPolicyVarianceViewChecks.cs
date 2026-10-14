namespace Acme.Workforce.Absence.Tests.Policy.Profile;

using Acme.Workforce.Absence.Policy.Profile;

/// <summary>Checks of profile code, run by the project's runner.</summary>
public static class RegionalPolicyVarianceViewChecks
{
    public static void RegionalPolicyVarianceViewShowsTheStatusInCapitals()
    {
        var item = RegionalPolicyVariance.NewRegionalPolicyVariance("code-735", "name-224");
        var view = RegionalPolicyVarianceView.ToRegionalPolicyVarianceView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(RegionalPolicyVarianceView.ToRegionalPolicyVarianceViews(new List<RegionalPolicyVariance> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        RegionalPolicyVarianceViewShowsTheStatusInCapitals();
    }
}
