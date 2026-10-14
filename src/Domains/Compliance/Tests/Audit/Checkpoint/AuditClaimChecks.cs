namespace Acme.Workforce.Compliance.Tests.Audit.Checkpoint;

using Acme.Workforce.Compliance.Audit.Checkpoint;

/// <summary>Checks of checkpoint code, run by the project's runner.</summary>
public static class AuditClaimChecks
{
    public static void AuditClaimStartsInIdle()
    {
        var item = AuditClaim.NewAuditClaim("code-761", "name-660");
        Check.Equal(item.Status, AuditClaimStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void AuditClaimMovesToStopped()
    {
        var item = AuditClaim.NewAuditClaim("code-761", "name-660");
        try
        {
            item.MoveTo(AuditClaimStatus.Stopped);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, AuditClaimStatus.Stopped, "status");
    }

    public static void AuditClaimRejectsMovesFromStopped()
    {
        var item = AuditClaim.NewAuditClaim("code-761", "name-660");
        item.Status = AuditClaimStatus.Stopped;
        Check.Fails(() => item.MoveTo(AuditClaimStatus.Idle), "move");
    }

    public static void RunAll()
    {
        AuditClaimStartsInIdle();
        AuditClaimMovesToStopped();
        AuditClaimRejectsMovesFromStopped();
    }
}
