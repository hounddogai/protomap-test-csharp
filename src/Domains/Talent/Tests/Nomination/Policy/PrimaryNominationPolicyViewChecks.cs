namespace Acme.Workforce.Talent.Tests.Nomination.Policy;

using Acme.Workforce.Talent.Nomination.Policy;

/// <summary>Checks of policy code, run by the project's runner.</summary>
public static class PrimaryNominationPolicyViewChecks
{
    public static void PrimaryNominationPolicyViewShowsTheStatusInCapitals()
    {
        var item = PrimaryNominationPolicy.NewPrimaryNominationPolicy("code-319", "title-30");
        var view = PrimaryNominationPolicyView.ToPrimaryNominationPolicyView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(PrimaryNominationPolicyView.ToPrimaryNominationPolicyViews(new List<PrimaryNominationPolicy> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        PrimaryNominationPolicyViewShowsTheStatusInCapitals();
    }
}
