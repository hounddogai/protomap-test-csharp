namespace Acme.Workforce.Absence.Tests.Balance.Proposal;

using Acme.Workforce.Absence.Balance.Proposal;

/// <summary>Checks of proposal code, run by the project's runner.</summary>
public static class RollingBalanceProposalViewChecks
{
    public static void RollingBalanceProposalViewShowsTheStatusInCapitals()
    {
        var item = RollingBalanceProposal.NewRollingBalanceProposal("code-741", "label-939");
        var view = RollingBalanceProposalView.ToRollingBalanceProposalView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(RollingBalanceProposalView.ToRollingBalanceProposalViews(new List<RollingBalanceProposal> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        RollingBalanceProposalViewShowsTheStatusInCapitals();
    }
}
