namespace Acme.Workforce.Compliance.Tests.Attestation.Template;

using Acme.Workforce.Compliance.Attestation.Template;

/// <summary>Checks of template code, run by the project's runner.</summary>
public static class CappedAttestationAuditViewChecks
{
    public static void CappedAttestationAuditViewShowsTheStatusInCapitals()
    {
        var item = CappedAttestationAudit.NewCappedAttestationAudit("code-37", "label-662");
        var view = CappedAttestationAuditView.ToCappedAttestationAuditView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(CappedAttestationAuditView.ToCappedAttestationAuditViews(new List<CappedAttestationAudit> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        CappedAttestationAuditViewShowsTheStatusInCapitals();
    }
}
