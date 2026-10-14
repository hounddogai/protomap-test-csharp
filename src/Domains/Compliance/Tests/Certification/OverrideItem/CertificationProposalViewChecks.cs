namespace Acme.Workforce.Compliance.Tests.Certification.OverrideItem;

using Acme.Workforce.Compliance.Certification.OverrideItem;

/// <summary>Checks of override item code, run by the project's runner.</summary>
public static class CertificationProposalViewChecks
{
    public static void CertificationProposalViewShowsTheStatusInCapitals()
    {
        var item = CertificationProposal.NewCertificationProposal("code-599", "name-394");
        var view = CertificationProposalView.ToCertificationProposalView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(CertificationProposalView.ToCertificationProposalViews(new List<CertificationProposal> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        CertificationProposalViewShowsTheStatusInCapitals();
    }
}
