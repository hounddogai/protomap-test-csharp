namespace Acme.Workforce.Compliance.Tests.Audit.Adjustment;

using Acme.Workforce.Compliance.Audit.Adjustment;

/// <summary>Checks of adjustment code, run by the project's runner.</summary>
public static class RegionalAuditRecordChecks
{
    public static void RegionalAuditRecordStartsInDraft()
    {
        var item = RegionalAuditRecord.NewRegionalAuditRecord("code-303", "label-653");
        Check.Equal(item.Status, RegionalAuditRecordStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void RegionalAuditRecordMovesToActive()
    {
        var item = RegionalAuditRecord.NewRegionalAuditRecord("code-303", "label-653");
        try
        {
            item.MoveTo(RegionalAuditRecordStatus.Active);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, RegionalAuditRecordStatus.Active, "status");
    }

    public static void RegionalAuditRecordRejectsMovesFromClosed()
    {
        var item = RegionalAuditRecord.NewRegionalAuditRecord("code-303", "label-653");
        item.Status = RegionalAuditRecordStatus.Closed;
        Check.Fails(() => item.MoveTo(RegionalAuditRecordStatus.Draft), "move");
    }

    public static void RunAll()
    {
        RegionalAuditRecordStartsInDraft();
        RegionalAuditRecordMovesToActive();
        RegionalAuditRecordRejectsMovesFromClosed();
    }
}
