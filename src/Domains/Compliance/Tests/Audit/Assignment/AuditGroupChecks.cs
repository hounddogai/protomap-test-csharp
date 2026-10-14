namespace Acme.Workforce.Compliance.Tests.Audit.Assignment;

using Acme.Workforce.Compliance.Audit.Assignment;

/// <summary>Checks of assignment code, run by the project's runner.</summary>
public static class AuditGroupChecks
{
    public static void AuditGroupStartsInIdle()
    {
        var item = AuditGroup.NewAuditGroup("code-325", "label-974");
        Check.Equal(item.Status, AuditGroupStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void AuditGroupMovesToWarming()
    {
        var item = AuditGroup.NewAuditGroup("code-325", "label-974");
        try
        {
            item.MoveTo(AuditGroupStatus.Warming);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, AuditGroupStatus.Warming, "status");
    }

    public static void AuditGroupRejectsMovesFromStopped()
    {
        var item = AuditGroup.NewAuditGroup("code-325", "label-974");
        item.Status = AuditGroupStatus.Stopped;
        Check.Fails(() => item.MoveTo(AuditGroupStatus.Idle), "move");
    }

    public static void RunAll()
    {
        AuditGroupStartsInIdle();
        AuditGroupMovesToWarming();
        AuditGroupRejectsMovesFromStopped();
    }
}
