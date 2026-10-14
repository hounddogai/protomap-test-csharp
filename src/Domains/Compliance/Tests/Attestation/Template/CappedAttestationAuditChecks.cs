namespace Acme.Workforce.Compliance.Tests.Attestation.Template;

using Acme.Workforce.Compliance.Attestation.Template;

/// <summary>Checks of template code, run by the project's runner.</summary>
public static class CappedAttestationAuditChecks
{
    public static void CappedAttestationAuditStartsInNew()
    {
        var item = CappedAttestationAudit.NewCappedAttestationAudit("code-37", "label-662");
        Check.Equal(item.Status, CappedAttestationAuditStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void CappedAttestationAuditMovesToDone()
    {
        var item = CappedAttestationAudit.NewCappedAttestationAudit("code-37", "label-662");
        try
        {
            item.MoveTo(CappedAttestationAuditStatus.Done);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, CappedAttestationAuditStatus.Done, "status");
    }

    public static void CappedAttestationAuditRejectsMovesFromFailed()
    {
        var item = CappedAttestationAudit.NewCappedAttestationAudit("code-37", "label-662");
        item.Status = CappedAttestationAuditStatus.Failed;
        Check.Fails(() => item.MoveTo(CappedAttestationAuditStatus.New), "move");
    }

    public static void RunAll()
    {
        CappedAttestationAuditStartsInNew();
        CappedAttestationAuditMovesToDone();
        CappedAttestationAuditRejectsMovesFromFailed();
    }
}
