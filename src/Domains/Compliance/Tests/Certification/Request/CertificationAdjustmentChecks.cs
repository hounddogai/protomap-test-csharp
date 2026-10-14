namespace Acme.Workforce.Compliance.Tests.Certification.Request;

using Acme.Workforce.Compliance.Certification.Request;

/// <summary>Checks of request code, run by the project's runner.</summary>
public static class CertificationAdjustmentChecks
{
    public static void CertificationAdjustmentStartsInRequested()
    {
        var item = CertificationAdjustment.NewCertificationAdjustment("code-331", "label-30");
        Check.Equal(item.Status, CertificationAdjustmentStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void CertificationAdjustmentMovesToGranted()
    {
        var item = CertificationAdjustment.NewCertificationAdjustment("code-331", "label-30");
        try
        {
            item.MoveTo(CertificationAdjustmentStatus.Granted);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, CertificationAdjustmentStatus.Granted, "status");
    }

    public static void CertificationAdjustmentRejectsMovesFromExpired()
    {
        var item = CertificationAdjustment.NewCertificationAdjustment("code-331", "label-30");
        item.Status = CertificationAdjustmentStatus.Expired;
        Check.Fails(() => item.MoveTo(CertificationAdjustmentStatus.Requested), "move");
    }

    public static void RunAll()
    {
        CertificationAdjustmentStartsInRequested();
        CertificationAdjustmentMovesToGranted();
        CertificationAdjustmentRejectsMovesFromExpired();
    }
}
