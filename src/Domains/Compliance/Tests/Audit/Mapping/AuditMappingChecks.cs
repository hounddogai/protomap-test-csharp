namespace Acme.Workforce.Compliance.Tests.Audit.Mapping;

using Acme.Workforce.Compliance.Audit.Mapping;

/// <summary>Checks of mapping code, run by the project's runner.</summary>
public static class AuditMappingChecks
{
    public static void AuditMappingStartsInPlanned()
    {
        var item = AuditMapping.NewAuditMapping("code-918", "name-9");
        Check.Equal(item.Status, AuditMappingStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void AuditMappingMovesToScheduled()
    {
        var item = AuditMapping.NewAuditMapping("code-918", "name-9");
        try
        {
            item.MoveTo(AuditMappingStatus.Scheduled);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, AuditMappingStatus.Scheduled, "status");
    }

    public static void AuditMappingRejectsMovesFromCancelled()
    {
        var item = AuditMapping.NewAuditMapping("code-918", "name-9");
        item.Status = AuditMappingStatus.Cancelled;
        Check.Fails(() => item.MoveTo(AuditMappingStatus.Planned), "move");
    }

    public static void RunAll()
    {
        AuditMappingStartsInPlanned();
        AuditMappingMovesToScheduled();
        AuditMappingRejectsMovesFromCancelled();
    }
}
