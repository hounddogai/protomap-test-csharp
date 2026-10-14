namespace Acme.Workforce.Compliance.Tests.Certification.Threshold;

using Acme.Workforce.Compliance.Certification.Threshold;

/// <summary>Checks of threshold code, run by the project's runner.</summary>
public static class CertificationThresholdChecks
{
    public static void CertificationThresholdStartsInNew()
    {
        var item = CertificationThreshold.NewCertificationThreshold("code-699", "title-493");
        Check.Equal(item.Status, CertificationThresholdStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void CertificationThresholdMovesToRunning()
    {
        var item = CertificationThreshold.NewCertificationThreshold("code-699", "title-493");
        try
        {
            item.MoveTo(CertificationThresholdStatus.Running);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, CertificationThresholdStatus.Running, "status");
    }

    public static void CertificationThresholdRejectsMovesFromFailed()
    {
        var item = CertificationThreshold.NewCertificationThreshold("code-699", "title-493");
        item.Status = CertificationThresholdStatus.Failed;
        Check.Fails(() => item.MoveTo(CertificationThresholdStatus.New), "move");
    }

    public static void RunAll()
    {
        CertificationThresholdStartsInNew();
        CertificationThresholdMovesToRunning();
        CertificationThresholdRejectsMovesFromFailed();
    }
}
