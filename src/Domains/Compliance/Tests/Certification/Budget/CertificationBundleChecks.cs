namespace Acme.Workforce.Compliance.Tests.Certification.Budget;

using Acme.Workforce.Compliance.Certification.Budget;

/// <summary>Checks of budget code, run by the project's runner.</summary>
public static class CertificationBundleChecks
{
    public static void CertificationBundleStartsInRequested()
    {
        var item = CertificationBundle.NewCertificationBundle("code-348", "label-340");
        Check.Equal(item.Status, CertificationBundleStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void CertificationBundleMovesToExpired()
    {
        var item = CertificationBundle.NewCertificationBundle("code-348", "label-340");
        try
        {
            item.MoveTo(CertificationBundleStatus.Expired);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, CertificationBundleStatus.Expired, "status");
    }

    public static void CertificationBundleRejectsMovesFromExpired()
    {
        var item = CertificationBundle.NewCertificationBundle("code-348", "label-340");
        item.Status = CertificationBundleStatus.Expired;
        Check.Fails(() => item.MoveTo(CertificationBundleStatus.Requested), "move");
    }

    public static void RunAll()
    {
        CertificationBundleStartsInRequested();
        CertificationBundleMovesToExpired();
        CertificationBundleRejectsMovesFromExpired();
    }
}
