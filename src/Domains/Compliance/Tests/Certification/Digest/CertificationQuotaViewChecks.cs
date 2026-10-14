namespace Acme.Workforce.Compliance.Tests.Certification.Digest;

using Acme.Workforce.Compliance.Certification.Digest;

/// <summary>Checks of digest code, run by the project's runner.</summary>
public static class CertificationQuotaViewChecks
{
    public static void CertificationQuotaViewShowsTheStatusInCapitals()
    {
        var item = CertificationQuota.NewCertificationQuota("code-204", "title-251");
        var view = CertificationQuotaView.ToCertificationQuotaView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(CertificationQuotaView.ToCertificationQuotaViews(new List<CertificationQuota> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        CertificationQuotaViewShowsTheStatusInCapitals();
    }
}
