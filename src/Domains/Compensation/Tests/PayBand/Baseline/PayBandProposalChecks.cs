namespace Acme.Workforce.Compensation.Tests.PayBand.Baseline;

using Acme.Workforce.Compensation.PayBand.Baseline;

/// <summary>Checks of baseline code, run by the project's runner.</summary>
public static class PayBandProposalChecks
{
    public static void PayBandProposalStartsInDraft()
    {
        var item = PayBandProposal.NewPayBandProposal("code-921", "title-608");
        Check.Equal(item.Status, PayBandProposalStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PayBandProposalMovesToPending()
    {
        var item = PayBandProposal.NewPayBandProposal("code-921", "title-608");
        try
        {
            item.MoveTo(PayBandProposalStatus.Pending);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PayBandProposalStatus.Pending, "status");
    }

    public static void PayBandProposalRejectsMovesFromClosed()
    {
        var item = PayBandProposal.NewPayBandProposal("code-921", "title-608");
        item.Status = PayBandProposalStatus.Closed;
        Check.Fails(() => item.MoveTo(PayBandProposalStatus.Draft), "move");
    }

    public static void RunAll()
    {
        PayBandProposalStartsInDraft();
        PayBandProposalMovesToPending();
        PayBandProposalRejectsMovesFromClosed();
    }
}
