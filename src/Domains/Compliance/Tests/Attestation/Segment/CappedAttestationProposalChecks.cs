namespace Acme.Workforce.Compliance.Tests.Attestation.Segment;

using Acme.Workforce.Compliance.Attestation.Segment;

/// <summary>Checks of segment code, run by the project's runner.</summary>
public static class CappedAttestationProposalChecks
{
    public static void CappedAttestationProposalStartsInOpen()
    {
        var item = CappedAttestationProposal.NewCappedAttestationProposal("code-794", "title-827");
        Check.Equal(item.Status, CappedAttestationProposalStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void CappedAttestationProposalMovesToReopened()
    {
        var item = CappedAttestationProposal.NewCappedAttestationProposal("code-794", "title-827");
        try
        {
            item.MoveTo(CappedAttestationProposalStatus.Reopened);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, CappedAttestationProposalStatus.Reopened, "status");
    }

    public static void CappedAttestationProposalRejectsMovesFromArchived()
    {
        var item = CappedAttestationProposal.NewCappedAttestationProposal("code-794", "title-827");
        item.Status = CappedAttestationProposalStatus.Archived;
        Check.Fails(() => item.MoveTo(CappedAttestationProposalStatus.Open), "move");
    }

    public static void RunAll()
    {
        CappedAttestationProposalStartsInOpen();
        CappedAttestationProposalMovesToReopened();
        CappedAttestationProposalRejectsMovesFromArchived();
    }
}
