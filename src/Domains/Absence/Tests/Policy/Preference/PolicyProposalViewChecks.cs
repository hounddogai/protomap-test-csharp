namespace Acme.Workforce.Absence.Tests.Policy.Preference;

using Acme.Workforce.Absence.Policy.Preference;

/// <summary>Checks of preference code, run by the project's runner.</summary>
public static class PolicyProposalViewChecks
{
    public static void PolicyProposalViewShowsTheStatusInCapitals()
    {
        var item = PolicyProposal.NewPolicyProposal("code-626", "name-613");
        var view = PolicyProposalView.ToPolicyProposalView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(PolicyProposalView.ToPolicyProposalViews(new List<PolicyProposal> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        PolicyProposalViewShowsTheStatusInCapitals();
    }
}
