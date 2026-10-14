namespace Acme.Workforce.Payrollops.Tests.RetroPay.Batch;

using Acme.Workforce.Payrollops.RetroPay.Batch;

/// <summary>Checks of batch code, run by the project's runner.</summary>
public static class CappedRetroPayProposalChecks
{
    public static void CappedRetroPayProposalStartsInOpen()
    {
        var item = CappedRetroPayProposal.NewCappedRetroPayProposal("code-560", "title-351");
        Check.Equal(item.Status, CappedRetroPayProposalStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void CappedRetroPayProposalMovesToArchived()
    {
        var item = CappedRetroPayProposal.NewCappedRetroPayProposal("code-560", "title-351");
        try
        {
            item.MoveTo(CappedRetroPayProposalStatus.Archived);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, CappedRetroPayProposalStatus.Archived, "status");
    }

    public static void CappedRetroPayProposalRejectsMovesFromArchived()
    {
        var item = CappedRetroPayProposal.NewCappedRetroPayProposal("code-560", "title-351");
        item.Status = CappedRetroPayProposalStatus.Archived;
        Check.Fails(() => item.MoveTo(CappedRetroPayProposalStatus.Open), "move");
    }

    public static void RunAll()
    {
        CappedRetroPayProposalStartsInOpen();
        CappedRetroPayProposalMovesToArchived();
        CappedRetroPayProposalRejectsMovesFromArchived();
    }
}
