namespace Acme.Workforce.Talent.Tests.Nomination.NominationPolicy;

using Acme.Workforce.Talent.Nomination.NominationPolicy;

/// <summary>Checks of nomination policy code, run by the project's runner.</summary>
public static class RegionalNominationPolicyViewChecks
{
    public static void RegionalNominationPolicyViewShowsTheStatusInCapitals()
    {
        var item = RegionalNominationPolicy.NewRegionalNominationPolicy("code-835", "name-358");
        var view = RegionalNominationPolicyView.ToRegionalNominationPolicyView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(RegionalNominationPolicyView.ToRegionalNominationPolicyViews(new List<RegionalNominationPolicy> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        RegionalNominationPolicyViewShowsTheStatusInCapitals();
    }
}
