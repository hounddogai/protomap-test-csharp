namespace Acme.Workforce.Compliance.Tests.Audit.Digest;

using Acme.Workforce.Compliance.Audit.Digest;

/// <summary>Checks of digest code, run by the project's runner.</summary>
public static class AuditQuotaChecks
{
    public static void AuditQuotaStartsInPlanned()
    {
        var item = AuditQuota.NewAuditQuota("code-589", "name-908");
        Check.Equal(item.Status, AuditQuotaStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void AuditQuotaMovesToInProgress()
    {
        var item = AuditQuota.NewAuditQuota("code-589", "name-908");
        try
        {
            item.MoveTo(AuditQuotaStatus.InProgress);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, AuditQuotaStatus.InProgress, "status");
    }

    public static void AuditQuotaRejectsMovesFromCancelled()
    {
        var item = AuditQuota.NewAuditQuota("code-589", "name-908");
        item.Status = AuditQuotaStatus.Cancelled;
        Check.Fails(() => item.MoveTo(AuditQuotaStatus.Planned), "move");
    }

    public static void RunAll()
    {
        AuditQuotaStartsInPlanned();
        AuditQuotaMovesToInProgress();
        AuditQuotaRejectsMovesFromCancelled();
    }
}
