namespace Acme.Workforce.Talent.Tests.Nomination.Policy;

using Acme.Workforce.Talent.Nomination.Policy;

/// <summary>Checks of policy code, run by the project's runner.</summary>
public static class NominationPolicyViewChecks
{
    public static void NominationPolicyViewShowsTheStatusInCapitals()
    {
        var item = NominationPolicy.NewNominationPolicy("code-334", "label-801");
        var view = NominationPolicyView.ToNominationPolicyView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(NominationPolicyView.ToNominationPolicyViews(new List<NominationPolicy> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        NominationPolicyViewShowsTheStatusInCapitals();
    }
}
