namespace Acme.Workforce.Compliance.Tests.Certification.CertificationOverride;

using Acme.Workforce.Compliance.Certification.CertificationOverride;

/// <summary>Checks of certification override code, run by the project's runner.</summary>
public static class CertificationProfileChecks
{
    public static void CertificationProfileStartsInOpen()
    {
        var item = CertificationProfile.NewCertificationProfile("code-601", "title-156");
        Check.Equal(item.Status, CertificationProfileStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void CertificationProfileMovesToArchived()
    {
        var item = CertificationProfile.NewCertificationProfile("code-601", "title-156");
        try
        {
            item.MoveTo(CertificationProfileStatus.Archived);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, CertificationProfileStatus.Archived, "status");
    }

    public static void CertificationProfileRejectsMovesFromArchived()
    {
        var item = CertificationProfile.NewCertificationProfile("code-601", "title-156");
        item.Status = CertificationProfileStatus.Archived;
        Check.Fails(() => item.MoveTo(CertificationProfileStatus.Open), "move");
    }

    public static void RunAll()
    {
        CertificationProfileStartsInOpen();
        CertificationProfileMovesToArchived();
        CertificationProfileRejectsMovesFromArchived();
    }
}
