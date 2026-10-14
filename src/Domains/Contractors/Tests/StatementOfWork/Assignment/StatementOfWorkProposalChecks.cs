namespace Acme.Workforce.Contractors.Tests.StatementOfWork.Assignment;

using Acme.Workforce.Contractors.StatementOfWork.Assignment;

/// <summary>Checks of assignment code, run by the project's runner.</summary>
public static class StatementOfWorkProposalChecks
{
    public static void StatementOfWorkProposalStartsInOpen()
    {
        var item = StatementOfWorkProposal.NewStatementOfWorkProposal("code-991", "title-860");
        Check.Equal(item.Status, StatementOfWorkProposalStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void StatementOfWorkProposalMovesToResolved()
    {
        var item = StatementOfWorkProposal.NewStatementOfWorkProposal("code-991", "title-860");
        try
        {
            item.MoveTo(StatementOfWorkProposalStatus.Resolved);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, StatementOfWorkProposalStatus.Resolved, "status");
    }

    public static void StatementOfWorkProposalRejectsMovesFromArchived()
    {
        var item = StatementOfWorkProposal.NewStatementOfWorkProposal("code-991", "title-860");
        item.Status = StatementOfWorkProposalStatus.Archived;
        Check.Fails(() => item.MoveTo(StatementOfWorkProposalStatus.Open), "move");
    }

    public static void RunAll()
    {
        StatementOfWorkProposalStartsInOpen();
        StatementOfWorkProposalMovesToResolved();
        StatementOfWorkProposalRejectsMovesFromArchived();
    }
}
