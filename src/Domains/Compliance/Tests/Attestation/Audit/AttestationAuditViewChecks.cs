namespace Acme.Workforce.Compliance.Tests.Attestation.Audit;

using Acme.Workforce.Compliance.Attestation.Audit;

/// <summary>Checks of audit code, run by the project's runner.</summary>
public static class AttestationAuditViewChecks
{
    public static void AttestationAuditViewShowsTheStatusInCapitals()
    {
        var item = AttestationAudit.NewAttestationAudit("code-445", "title-29");
        var view = AttestationAuditView.ToAttestationAuditView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(AttestationAuditView.ToAttestationAuditViews(new List<AttestationAudit> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        AttestationAuditViewShowsTheStatusInCapitals();
    }
}
