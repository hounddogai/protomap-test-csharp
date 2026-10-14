namespace Acme.Workforce.Payrollops.Tests.RetroPay.Proposal;

using Acme.Workforce.Payrollops.RetroPay.Proposal;

/// <summary>Checks of proposal code, run by the project's runner.</summary>
public static class RetroPayProposalChecks
{
    public static void RetroPayProposalStartsInNew()
    {
        var item = RetroPayProposal.NewRetroPayProposal("code-84", "name-444");
        Check.Equal(item.Status, RetroPayProposalStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void RetroPayProposalMovesToDone()
    {
        var item = RetroPayProposal.NewRetroPayProposal("code-84", "name-444");
        try
        {
            item.MoveTo(RetroPayProposalStatus.Done);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, RetroPayProposalStatus.Done, "status");
    }

    public static void RetroPayProposalRejectsMovesFromFailed()
    {
        var item = RetroPayProposal.NewRetroPayProposal("code-84", "name-444");
        item.Status = RetroPayProposalStatus.Failed;
        Check.Fails(() => item.MoveTo(RetroPayProposalStatus.New), "move");
    }

    public static void RunAll()
    {
        RetroPayProposalStartsInNew();
        RetroPayProposalMovesToDone();
        RetroPayProposalRejectsMovesFromFailed();
    }
}
