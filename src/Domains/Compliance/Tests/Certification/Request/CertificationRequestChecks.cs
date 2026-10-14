namespace Acme.Workforce.Compliance.Tests.Certification.Request;

using Acme.Workforce.Compliance.Certification.Request;

/// <summary>Checks of request code, run by the project's runner.</summary>
public static class CertificationRequestChecks
{
    public static void CertificationRequestStartsInOpen()
    {
        var item = CertificationRequest.NewCertificationRequest("code-463", "title-550");
        Check.Equal(item.Status, CertificationRequestStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void CertificationRequestMovesToReopened()
    {
        var item = CertificationRequest.NewCertificationRequest("code-463", "title-550");
        try
        {
            item.MoveTo(CertificationRequestStatus.Reopened);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, CertificationRequestStatus.Reopened, "status");
    }

    public static void CertificationRequestRejectsMovesFromArchived()
    {
        var item = CertificationRequest.NewCertificationRequest("code-463", "title-550");
        item.Status = CertificationRequestStatus.Archived;
        Check.Fails(() => item.MoveTo(CertificationRequestStatus.Open), "move");
    }

    public static void RunAll()
    {
        CertificationRequestStartsInOpen();
        CertificationRequestMovesToReopened();
        CertificationRequestRejectsMovesFromArchived();
    }
}
