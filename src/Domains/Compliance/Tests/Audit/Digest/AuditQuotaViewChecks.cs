namespace Acme.Workforce.Compliance.Tests.Audit.Digest;

using Acme.Workforce.Compliance.Audit.Digest;

/// <summary>Checks of digest code, run by the project's runner.</summary>
public static class AuditQuotaViewChecks
{
    public static void AuditQuotaViewShowsTheStatusInCapitals()
    {
        var item = AuditQuota.NewAuditQuota("code-589", "name-908");
        var view = AuditQuotaView.ToAuditQuotaView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(AuditQuotaView.ToAuditQuotaViews(new List<AuditQuota> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        AuditQuotaViewShowsTheStatusInCapitals();
    }
}
