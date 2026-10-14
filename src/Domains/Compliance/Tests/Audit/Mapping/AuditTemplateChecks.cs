namespace Acme.Workforce.Compliance.Tests.Audit.Mapping;

using Acme.Workforce.Compliance.Audit.Mapping;

/// <summary>Checks of mapping code, run by the project's runner.</summary>
public static class AuditTemplateChecks
{
    public static void AuditTemplateStartsInIdle()
    {
        var item = AuditTemplate.NewAuditTemplate("code-611", "title-116");
        Check.Equal(item.Status, AuditTemplateStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void AuditTemplateMovesToDraining()
    {
        var item = AuditTemplate.NewAuditTemplate("code-611", "title-116");
        try
        {
            item.MoveTo(AuditTemplateStatus.Draining);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, AuditTemplateStatus.Draining, "status");
    }

    public static void AuditTemplateRejectsMovesFromStopped()
    {
        var item = AuditTemplate.NewAuditTemplate("code-611", "title-116");
        item.Status = AuditTemplateStatus.Stopped;
        Check.Fails(() => item.MoveTo(AuditTemplateStatus.Idle), "move");
    }

    public static void RunAll()
    {
        AuditTemplateStartsInIdle();
        AuditTemplateMovesToDraining();
        AuditTemplateRejectsMovesFromStopped();
    }
}
