namespace Acme.Workforce.Compliance.Tests.Certification.Request;

using Acme.Workforce.Compliance.Certification.Request;

/// <summary>Checks of request code, run by the project's runner.</summary>
public static class CertificationEstimateChecks
{
    public static void CertificationEstimateStartsInNew()
    {
        var item = CertificationEstimate.NewCertificationEstimate("code-844", "title-663");
        Check.Equal(item.Status, CertificationEstimateStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void CertificationEstimateMovesToQueued()
    {
        var item = CertificationEstimate.NewCertificationEstimate("code-844", "title-663");
        try
        {
            item.MoveTo(CertificationEstimateStatus.Queued);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, CertificationEstimateStatus.Queued, "status");
    }

    public static void CertificationEstimateRejectsMovesFromFailed()
    {
        var item = CertificationEstimate.NewCertificationEstimate("code-844", "title-663");
        item.Status = CertificationEstimateStatus.Failed;
        Check.Fails(() => item.MoveTo(CertificationEstimateStatus.New), "move");
    }

    public static void RunAll()
    {
        CertificationEstimateStartsInNew();
        CertificationEstimateMovesToQueued();
        CertificationEstimateRejectsMovesFromFailed();
    }
}
