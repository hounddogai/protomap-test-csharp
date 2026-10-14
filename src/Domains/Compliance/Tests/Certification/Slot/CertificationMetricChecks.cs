namespace Acme.Workforce.Compliance.Tests.Certification.Slot;

using Acme.Workforce.Compliance.Certification.Slot;

/// <summary>Checks of slot code, run by the project's runner.</summary>
public static class CertificationMetricChecks
{
    public static void CertificationMetricStartsInProposed()
    {
        var item = CertificationMetric.NewCertificationMetric("code-898", "label-534");
        Check.Equal(item.Status, CertificationMetricStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void CertificationMetricMovesToRejected()
    {
        var item = CertificationMetric.NewCertificationMetric("code-898", "label-534");
        try
        {
            item.MoveTo(CertificationMetricStatus.Rejected);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, CertificationMetricStatus.Rejected, "status");
    }

    public static void CertificationMetricRejectsMovesFromWithdrawn()
    {
        var item = CertificationMetric.NewCertificationMetric("code-898", "label-534");
        item.Status = CertificationMetricStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(CertificationMetricStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        CertificationMetricStartsInProposed();
        CertificationMetricMovesToRejected();
        CertificationMetricRejectsMovesFromWithdrawn();
    }
}
