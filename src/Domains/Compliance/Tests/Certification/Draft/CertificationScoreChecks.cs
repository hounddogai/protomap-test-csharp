namespace Acme.Workforce.Compliance.Tests.Certification.Draft;

using Acme.Workforce.Compliance.Certification.Draft;

/// <summary>Checks of draft code, run by the project's runner.</summary>
public static class CertificationScoreChecks
{
    public static void CertificationScoreStartsInIdle()
    {
        var item = CertificationScore.NewCertificationScore("code-371", "label-440");
        Check.Equal(item.Status, CertificationScoreStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void CertificationScoreMovesToReady()
    {
        var item = CertificationScore.NewCertificationScore("code-371", "label-440");
        try
        {
            item.MoveTo(CertificationScoreStatus.Ready);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, CertificationScoreStatus.Ready, "status");
    }

    public static void CertificationScoreRejectsMovesFromStopped()
    {
        var item = CertificationScore.NewCertificationScore("code-371", "label-440");
        item.Status = CertificationScoreStatus.Stopped;
        Check.Fails(() => item.MoveTo(CertificationScoreStatus.Idle), "move");
    }

    public static void RunAll()
    {
        CertificationScoreStartsInIdle();
        CertificationScoreMovesToReady();
        CertificationScoreRejectsMovesFromStopped();
    }
}
