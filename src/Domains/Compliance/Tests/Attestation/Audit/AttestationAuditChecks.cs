namespace Acme.Workforce.Compliance.Tests.Attestation.Audit;

using Acme.Workforce.Compliance.Attestation.Audit;

/// <summary>Checks of audit code, run by the project's runner.</summary>
public static class AttestationAuditChecks
{
    public static void AttestationAuditStartsInIdle()
    {
        var item = AttestationAudit.NewAttestationAudit("code-445", "title-29");
        Check.Equal(item.Status, AttestationAuditStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void AttestationAuditMovesToWarming()
    {
        var item = AttestationAudit.NewAttestationAudit("code-445", "title-29");
        try
        {
            item.MoveTo(AttestationAuditStatus.Warming);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, AttestationAuditStatus.Warming, "status");
    }

    public static void AttestationAuditRejectsMovesFromStopped()
    {
        var item = AttestationAudit.NewAttestationAudit("code-445", "title-29");
        item.Status = AttestationAuditStatus.Stopped;
        Check.Fails(() => item.MoveTo(AttestationAuditStatus.Idle), "move");
    }

    public static void RunAll()
    {
        AttestationAuditStartsInIdle();
        AttestationAuditMovesToWarming();
        AttestationAuditRejectsMovesFromStopped();
    }
}
