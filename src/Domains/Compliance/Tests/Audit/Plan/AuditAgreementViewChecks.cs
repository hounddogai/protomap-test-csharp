namespace Acme.Workforce.Compliance.Tests.Audit.Plan;

using Acme.Workforce.Compliance.Audit.Plan;

/// <summary>Checks of plan code, run by the project's runner.</summary>
public static class AuditAgreementViewChecks
{
    public static void AuditAgreementViewShowsTheStatusInCapitals()
    {
        var item = AuditAgreement.NewAuditAgreement("code-139", "name-677");
        var view = AuditAgreementView.ToAuditAgreementView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(AuditAgreementView.ToAuditAgreementViews(new List<AuditAgreement> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        AuditAgreementViewShowsTheStatusInCapitals();
    }
}
