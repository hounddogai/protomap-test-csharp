namespace Acme.Workforce.Compliance.Tests.Certification.OverrideItem;

using Acme.Workforce.Compliance.Certification.OverrideItem;

/// <summary>Checks of override item code, run by the project's runner.</summary>
public static class CertificationAuditChecks
{
    public static void CertificationAuditStartsInProposed()
    {
        var item = CertificationAudit.NewCertificationAudit("code-260", "label-397");
        Check.Equal(item.Status, CertificationAuditStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void CertificationAuditMovesToWithdrawn()
    {
        var item = CertificationAudit.NewCertificationAudit("code-260", "label-397");
        try
        {
            item.MoveTo(CertificationAuditStatus.Withdrawn);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, CertificationAuditStatus.Withdrawn, "status");
    }

    public static void CertificationAuditRejectsMovesFromWithdrawn()
    {
        var item = CertificationAudit.NewCertificationAudit("code-260", "label-397");
        item.Status = CertificationAuditStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(CertificationAuditStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        CertificationAuditStartsInProposed();
        CertificationAuditMovesToWithdrawn();
        CertificationAuditRejectsMovesFromWithdrawn();
    }
}
