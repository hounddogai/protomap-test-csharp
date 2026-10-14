namespace Acme.Workforce.Compliance.Tests.Certification.Claim;

using Acme.Workforce.Compliance.Certification.Claim;

/// <summary>Checks of claim code, run by the project's runner.</summary>
public static class CertificationExceptionChecks
{
    public static void CertificationExceptionStartsInCreated()
    {
        var item = CertificationException.NewCertificationException("code-695", "label-880");
        Check.Equal(item.Status, CertificationExceptionStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void CertificationExceptionMovesToPublished()
    {
        var item = CertificationException.NewCertificationException("code-695", "label-880");
        try
        {
            item.MoveTo(CertificationExceptionStatus.Published);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, CertificationExceptionStatus.Published, "status");
    }

    public static void CertificationExceptionRejectsMovesFromRetired()
    {
        var item = CertificationException.NewCertificationException("code-695", "label-880");
        item.Status = CertificationExceptionStatus.Retired;
        Check.Fails(() => item.MoveTo(CertificationExceptionStatus.Created), "move");
    }

    public static void RunAll()
    {
        CertificationExceptionStartsInCreated();
        CertificationExceptionMovesToPublished();
        CertificationExceptionRejectsMovesFromRetired();
    }
}
