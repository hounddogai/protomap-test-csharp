namespace Acme.Workforce.Compliance.Tests.Certification.OverrideItem;

using Acme.Workforce.Compliance.Certification.OverrideItem;

/// <summary>Checks of override item code, run by the project's runner.</summary>
public static class CertificationProposalChecks
{
    public static void CertificationProposalStartsInNew()
    {
        var item = CertificationProposal.NewCertificationProposal("code-599", "name-394");
        Check.Equal(item.Status, CertificationProposalStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void CertificationProposalMovesToQueued()
    {
        var item = CertificationProposal.NewCertificationProposal("code-599", "name-394");
        try
        {
            item.MoveTo(CertificationProposalStatus.Queued);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, CertificationProposalStatus.Queued, "status");
    }

    public static void CertificationProposalRejectsMovesFromFailed()
    {
        var item = CertificationProposal.NewCertificationProposal("code-599", "name-394");
        item.Status = CertificationProposalStatus.Failed;
        Check.Fails(() => item.MoveTo(CertificationProposalStatus.New), "move");
    }

    public static void RunAll()
    {
        CertificationProposalStartsInNew();
        CertificationProposalMovesToQueued();
        CertificationProposalRejectsMovesFromFailed();
    }
}
