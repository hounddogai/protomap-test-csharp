namespace Acme.Workforce.Compliance.Tests.Audit.Slot;

using Acme.Workforce.Compliance.Audit.Slot;

/// <summary>Checks of slot code, run by the project's runner.</summary>
public static class AuditScoreChecks
{
    public static void AuditScoreStartsInIdle()
    {
        var item = AuditScore.NewAuditScore("code-886", "name-253");
        Check.Equal(item.Status, AuditScoreStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void AuditScoreMovesToWarming()
    {
        var item = AuditScore.NewAuditScore("code-886", "name-253");
        try
        {
            item.MoveTo(AuditScoreStatus.Warming);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, AuditScoreStatus.Warming, "status");
    }

    public static void AuditScoreRejectsMovesFromStopped()
    {
        var item = AuditScore.NewAuditScore("code-886", "name-253");
        item.Status = AuditScoreStatus.Stopped;
        Check.Fails(() => item.MoveTo(AuditScoreStatus.Idle), "move");
    }

    public static void RunAll()
    {
        AuditScoreStartsInIdle();
        AuditScoreMovesToWarming();
        AuditScoreRejectsMovesFromStopped();
    }
}
