namespace Acme.Workforce.Compliance.Tests.Certification.Digest;

using Acme.Workforce.Compliance.Certification.Digest;

/// <summary>Checks of digest code, run by the project's runner.</summary>
public static class CertificationQuotaChecks
{
    public static void CertificationQuotaStartsInCreated()
    {
        var item = CertificationQuota.NewCertificationQuota("code-204", "title-251");
        Check.Equal(item.Status, CertificationQuotaStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void CertificationQuotaMovesToValidated()
    {
        var item = CertificationQuota.NewCertificationQuota("code-204", "title-251");
        try
        {
            item.MoveTo(CertificationQuotaStatus.Validated);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, CertificationQuotaStatus.Validated, "status");
    }

    public static void CertificationQuotaRejectsMovesFromRetired()
    {
        var item = CertificationQuota.NewCertificationQuota("code-204", "title-251");
        item.Status = CertificationQuotaStatus.Retired;
        Check.Fails(() => item.MoveTo(CertificationQuotaStatus.Created), "move");
    }

    public static void RunAll()
    {
        CertificationQuotaStartsInCreated();
        CertificationQuotaMovesToValidated();
        CertificationQuotaRejectsMovesFromRetired();
    }
}
