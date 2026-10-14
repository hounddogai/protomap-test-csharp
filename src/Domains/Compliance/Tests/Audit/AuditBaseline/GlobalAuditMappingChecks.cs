namespace Acme.Workforce.Compliance.Tests.Audit.AuditBaseline;

using Acme.Workforce.Compliance.Audit.AuditBaseline;

/// <summary>Checks of audit baseline code, run by the project's runner.</summary>
public static class GlobalAuditMappingChecks
{
    public static void GlobalAuditMappingStartsInIdle()
    {
        var item = GlobalAuditMapping.NewGlobalAuditMapping("code-719", "name-356");
        Check.Equal(item.Status, GlobalAuditMappingStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void GlobalAuditMappingMovesToDraining()
    {
        var item = GlobalAuditMapping.NewGlobalAuditMapping("code-719", "name-356");
        try
        {
            item.MoveTo(GlobalAuditMappingStatus.Draining);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, GlobalAuditMappingStatus.Draining, "status");
    }

    public static void GlobalAuditMappingRejectsMovesFromStopped()
    {
        var item = GlobalAuditMapping.NewGlobalAuditMapping("code-719", "name-356");
        item.Status = GlobalAuditMappingStatus.Stopped;
        Check.Fails(() => item.MoveTo(GlobalAuditMappingStatus.Idle), "move");
    }

    public static void RunAll()
    {
        GlobalAuditMappingStartsInIdle();
        GlobalAuditMappingMovesToDraining();
        GlobalAuditMappingRejectsMovesFromStopped();
    }
}
