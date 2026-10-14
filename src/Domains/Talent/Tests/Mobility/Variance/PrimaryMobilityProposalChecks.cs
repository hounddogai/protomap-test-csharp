namespace Acme.Workforce.Talent.Tests.Mobility.Variance;

using Acme.Workforce.Talent.Mobility.Variance;

/// <summary>Checks of variance code, run by the project's runner.</summary>
public static class PrimaryMobilityProposalChecks
{
    public static void PrimaryMobilityProposalStartsInDraft()
    {
        var item = PrimaryMobilityProposal.NewPrimaryMobilityProposal("code-902", "name-684");
        Check.Equal(item.Status, PrimaryMobilityProposalStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PrimaryMobilityProposalMovesToActive()
    {
        var item = PrimaryMobilityProposal.NewPrimaryMobilityProposal("code-902", "name-684");
        try
        {
            item.MoveTo(PrimaryMobilityProposalStatus.Active);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PrimaryMobilityProposalStatus.Active, "status");
    }

    public static void PrimaryMobilityProposalRejectsMovesFromClosed()
    {
        var item = PrimaryMobilityProposal.NewPrimaryMobilityProposal("code-902", "name-684");
        item.Status = PrimaryMobilityProposalStatus.Closed;
        Check.Fails(() => item.MoveTo(PrimaryMobilityProposalStatus.Draft), "move");
    }

    public static void RunAll()
    {
        PrimaryMobilityProposalStartsInDraft();
        PrimaryMobilityProposalMovesToActive();
        PrimaryMobilityProposalRejectsMovesFromClosed();
    }
}
