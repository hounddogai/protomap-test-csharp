namespace Acme.Workforce.Contractors.Tests.StatementOfWork.Cycle;

using Acme.Workforce.Contractors.StatementOfWork.Cycle;

/// <summary>Checks of cycle code, run by the project's runner.</summary>
public static class CappedStatementOfWorkProposalChecks
{
    public static void CappedStatementOfWorkProposalStartsInProposed()
    {
        var item = CappedStatementOfWorkProposal.NewCappedStatementOfWorkProposal("code-530", "name-991");
        Check.Equal(item.Status, CappedStatementOfWorkProposalStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void CappedStatementOfWorkProposalMovesToWithdrawn()
    {
        var item = CappedStatementOfWorkProposal.NewCappedStatementOfWorkProposal("code-530", "name-991");
        try
        {
            item.MoveTo(CappedStatementOfWorkProposalStatus.Withdrawn);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, CappedStatementOfWorkProposalStatus.Withdrawn, "status");
    }

    public static void CappedStatementOfWorkProposalRejectsMovesFromWithdrawn()
    {
        var item = CappedStatementOfWorkProposal.NewCappedStatementOfWorkProposal("code-530", "name-991");
        item.Status = CappedStatementOfWorkProposalStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(CappedStatementOfWorkProposalStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        CappedStatementOfWorkProposalStartsInProposed();
        CappedStatementOfWorkProposalMovesToWithdrawn();
        CappedStatementOfWorkProposalRejectsMovesFromWithdrawn();
    }
}
