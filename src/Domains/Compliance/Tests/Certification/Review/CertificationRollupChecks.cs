namespace Acme.Workforce.Compliance.Tests.Certification.Review;

using Acme.Workforce.Compliance.Certification.Review;

/// <summary>Checks of review code, run by the project's runner.</summary>
public static class CertificationRollupChecks
{
    public static void CertificationRollupStartsInProposed()
    {
        var item = CertificationRollup.NewCertificationRollup("code-634", "label-832");
        Check.Equal(item.Status, CertificationRollupStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void CertificationRollupMovesToRejected()
    {
        var item = CertificationRollup.NewCertificationRollup("code-634", "label-832");
        try
        {
            item.MoveTo(CertificationRollupStatus.Rejected);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, CertificationRollupStatus.Rejected, "status");
    }

    public static void CertificationRollupRejectsMovesFromWithdrawn()
    {
        var item = CertificationRollup.NewCertificationRollup("code-634", "label-832");
        item.Status = CertificationRollupStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(CertificationRollupStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        CertificationRollupStartsInProposed();
        CertificationRollupMovesToRejected();
        CertificationRollupRejectsMovesFromWithdrawn();
    }
}
