namespace Acme.Workforce.Compliance.Tests.Audit.AuditMapping;

using Acme.Workforce.Compliance.Audit.AuditMapping;

/// <summary>Checks of audit mapping code, run by the project's runner.</summary>
public static class DefaultAuditMappingChecks
{
    public static void DefaultAuditMappingStartsInDraft()
    {
        var item = DefaultAuditMapping.NewDefaultAuditMapping("code-392", "name-813");
        Check.Equal(item.Status, DefaultAuditMappingStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void DefaultAuditMappingMovesToPending()
    {
        var item = DefaultAuditMapping.NewDefaultAuditMapping("code-392", "name-813");
        try
        {
            item.MoveTo(DefaultAuditMappingStatus.Pending);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, DefaultAuditMappingStatus.Pending, "status");
    }

    public static void DefaultAuditMappingRejectsMovesFromClosed()
    {
        var item = DefaultAuditMapping.NewDefaultAuditMapping("code-392", "name-813");
        item.Status = DefaultAuditMappingStatus.Closed;
        Check.Fails(() => item.MoveTo(DefaultAuditMappingStatus.Draft), "move");
    }

    public static void RunAll()
    {
        DefaultAuditMappingStartsInDraft();
        DefaultAuditMappingMovesToPending();
        DefaultAuditMappingRejectsMovesFromClosed();
    }
}
