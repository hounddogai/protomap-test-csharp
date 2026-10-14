namespace Acme.Workforce.Compliance.Tests.Certification.Summary;

using Acme.Workforce.Compliance.Certification.Summary;

/// <summary>Checks of summary code, run by the project's runner.</summary>
public static class CertificationSummaryChecks
{
    public static void CertificationSummaryStartsInProposed()
    {
        var item = CertificationSummary.NewCertificationSummary("code-1", "label-362");
        Check.Equal(item.Status, CertificationSummaryStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void CertificationSummaryMovesToApproved()
    {
        var item = CertificationSummary.NewCertificationSummary("code-1", "label-362");
        try
        {
            item.MoveTo(CertificationSummaryStatus.Approved);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, CertificationSummaryStatus.Approved, "status");
    }

    public static void CertificationSummaryRejectsMovesFromWithdrawn()
    {
        var item = CertificationSummary.NewCertificationSummary("code-1", "label-362");
        item.Status = CertificationSummaryStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(CertificationSummaryStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        CertificationSummaryStartsInProposed();
        CertificationSummaryMovesToApproved();
        CertificationSummaryRejectsMovesFromWithdrawn();
    }
}
