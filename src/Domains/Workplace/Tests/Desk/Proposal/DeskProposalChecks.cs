namespace Acme.Workforce.Workplace.Tests.Desk.Proposal;

using Acme.Workforce.Workplace.Desk.Proposal;

/// <summary>Checks of proposal code, run by the project's runner.</summary>
public static class DeskProposalChecks
{
    public static void DeskProposalStartsInOpen()
    {
        var item = DeskProposal.NewDeskProposal("code-261", "title-890");
        Check.Equal(item.Status, DeskProposalStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void DeskProposalMovesToInReview()
    {
        var item = DeskProposal.NewDeskProposal("code-261", "title-890");
        try
        {
            item.MoveTo(DeskProposalStatus.InReview);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, DeskProposalStatus.InReview, "status");
    }

    public static void DeskProposalRejectsMovesFromArchived()
    {
        var item = DeskProposal.NewDeskProposal("code-261", "title-890");
        item.Status = DeskProposalStatus.Archived;
        Check.Fails(() => item.MoveTo(DeskProposalStatus.Open), "move");
    }

    public static void RunAll()
    {
        DeskProposalStartsInOpen();
        DeskProposalMovesToInReview();
        DeskProposalRejectsMovesFromArchived();
    }
}
