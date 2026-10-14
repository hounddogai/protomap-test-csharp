namespace Acme.Workforce.Workplace.Tests.Desk.Proposal;

using Acme.Workforce.Workplace.Desk.Proposal;

/// <summary>Checks of proposal code, run by the project's runner.</summary>
public static class DeskProposalViewChecks
{
    public static void DeskProposalViewShowsTheStatusInCapitals()
    {
        var item = DeskProposal.NewDeskProposal("code-261", "title-890");
        var view = DeskProposalView.ToDeskProposalView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(DeskProposalView.ToDeskProposalViews(new List<DeskProposal> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        DeskProposalViewShowsTheStatusInCapitals();
    }
}
