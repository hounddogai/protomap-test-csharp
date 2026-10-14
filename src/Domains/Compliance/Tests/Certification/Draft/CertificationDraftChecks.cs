namespace Acme.Workforce.Compliance.Tests.Certification.Draft;

using Acme.Workforce.Compliance.Certification.Draft;

/// <summary>Checks of draft code, run by the project's runner.</summary>
public static class CertificationDraftChecks
{
    public static void CertificationDraftStartsInIdle()
    {
        var item = CertificationDraft.NewCertificationDraft("code-89", "label-560");
        Check.Equal(item.Status, CertificationDraftStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void CertificationDraftMovesToReady()
    {
        var item = CertificationDraft.NewCertificationDraft("code-89", "label-560");
        try
        {
            item.MoveTo(CertificationDraftStatus.Ready);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, CertificationDraftStatus.Ready, "status");
    }

    public static void CertificationDraftRejectsMovesFromStopped()
    {
        var item = CertificationDraft.NewCertificationDraft("code-89", "label-560");
        item.Status = CertificationDraftStatus.Stopped;
        Check.Fails(() => item.MoveTo(CertificationDraftStatus.Idle), "move");
    }

    public static void RunAll()
    {
        CertificationDraftStartsInIdle();
        CertificationDraftMovesToReady();
        CertificationDraftRejectsMovesFromStopped();
    }
}
