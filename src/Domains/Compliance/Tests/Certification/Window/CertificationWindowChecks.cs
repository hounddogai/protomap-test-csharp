namespace Acme.Workforce.Compliance.Tests.Certification.Window;

using Acme.Workforce.Compliance.Certification.Window;

/// <summary>Checks of window code, run by the project's runner.</summary>
public static class CertificationWindowChecks
{
    public static void CertificationWindowStartsInNew()
    {
        var item = CertificationWindow.NewCertificationWindow("code-291", "label-464");
        Check.Equal(item.Status, CertificationWindowStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void CertificationWindowMovesToRunning()
    {
        var item = CertificationWindow.NewCertificationWindow("code-291", "label-464");
        try
        {
            item.MoveTo(CertificationWindowStatus.Running);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, CertificationWindowStatus.Running, "status");
    }

    public static void CertificationWindowRejectsMovesFromFailed()
    {
        var item = CertificationWindow.NewCertificationWindow("code-291", "label-464");
        item.Status = CertificationWindowStatus.Failed;
        Check.Fails(() => item.MoveTo(CertificationWindowStatus.New), "move");
    }

    public static void RunAll()
    {
        CertificationWindowStartsInNew();
        CertificationWindowMovesToRunning();
        CertificationWindowRejectsMovesFromFailed();
    }
}
