namespace Acme.Workforce.Talent.Tests.Mobility.Variance;

using Acme.Workforce.Talent.Mobility.Variance;

/// <summary>Checks of variance code, run by the project's runner.</summary>
public static class AdaptiveMobilityProposalChecks
{
    public static void AdaptiveMobilityProposalStartsInDraft()
    {
        var item = AdaptiveMobilityProposal.NewAdaptiveMobilityProposal("code-202", "name-401");
        Check.Equal(item.Status, AdaptiveMobilityProposalStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void AdaptiveMobilityProposalMovesToPending()
    {
        var item = AdaptiveMobilityProposal.NewAdaptiveMobilityProposal("code-202", "name-401");
        try
        {
            item.MoveTo(AdaptiveMobilityProposalStatus.Pending);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, AdaptiveMobilityProposalStatus.Pending, "status");
    }

    public static void AdaptiveMobilityProposalRejectsMovesFromClosed()
    {
        var item = AdaptiveMobilityProposal.NewAdaptiveMobilityProposal("code-202", "name-401");
        item.Status = AdaptiveMobilityProposalStatus.Closed;
        Check.Fails(() => item.MoveTo(AdaptiveMobilityProposalStatus.Draft), "move");
    }

    public static void RunAll()
    {
        AdaptiveMobilityProposalStartsInDraft();
        AdaptiveMobilityProposalMovesToPending();
        AdaptiveMobilityProposalRejectsMovesFromClosed();
    }
}
