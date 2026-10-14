namespace Acme.Workforce.Contractors.Tests.Rate.RecordItem;

using Acme.Workforce.Contractors.Rate.RecordItem;

/// <summary>Checks of record item code, run by the project's runner.</summary>
public static class RateProposalChecks
{
    public static void RateProposalStartsInDraft()
    {
        var item = RateProposal.NewRateProposal("code-613", "title-743");
        Check.Equal(item.Status, RateProposalStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void RateProposalMovesToSuspended()
    {
        var item = RateProposal.NewRateProposal("code-613", "title-743");
        try
        {
            item.MoveTo(RateProposalStatus.Suspended);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, RateProposalStatus.Suspended, "status");
    }

    public static void RateProposalRejectsMovesFromClosed()
    {
        var item = RateProposal.NewRateProposal("code-613", "title-743");
        item.Status = RateProposalStatus.Closed;
        Check.Fails(() => item.MoveTo(RateProposalStatus.Draft), "move");
    }

    public static void RunAll()
    {
        RateProposalStartsInDraft();
        RateProposalMovesToSuspended();
        RateProposalRejectsMovesFromClosed();
    }
}
