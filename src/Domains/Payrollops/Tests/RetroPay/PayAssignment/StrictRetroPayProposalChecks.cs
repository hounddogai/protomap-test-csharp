namespace Acme.Workforce.Payrollops.Tests.RetroPay.PayAssignment;

using Acme.Workforce.Payrollops.RetroPay.PayAssignment;

/// <summary>Checks of pay assignment code, run by the project's runner.</summary>
public static class StrictRetroPayProposalChecks
{
    public static void StrictRetroPayProposalStartsInDraft()
    {
        var item = StrictRetroPayProposal.NewStrictRetroPayProposal("code-586", "name-67");
        Check.Equal(item.Status, StrictRetroPayProposalStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void StrictRetroPayProposalMovesToPending()
    {
        var item = StrictRetroPayProposal.NewStrictRetroPayProposal("code-586", "name-67");
        try
        {
            item.MoveTo(StrictRetroPayProposalStatus.Pending);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, StrictRetroPayProposalStatus.Pending, "status");
    }

    public static void StrictRetroPayProposalRejectsMovesFromClosed()
    {
        var item = StrictRetroPayProposal.NewStrictRetroPayProposal("code-586", "name-67");
        item.Status = StrictRetroPayProposalStatus.Closed;
        Check.Fails(() => item.MoveTo(StrictRetroPayProposalStatus.Draft), "move");
    }

    public static void RunAll()
    {
        StrictRetroPayProposalStartsInDraft();
        StrictRetroPayProposalMovesToPending();
        StrictRetroPayProposalRejectsMovesFromClosed();
    }
}
