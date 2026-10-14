namespace Acme.Workforce.Payrollops.Tests.PayRun.Adjustment;

using Acme.Workforce.Payrollops.PayRun.Adjustment;

/// <summary>Checks of adjustment code, run by the project's runner.</summary>
public static class LegacyPayRunAuditChecks
{
    public static void LegacyPayRunAuditStartsInDraft()
    {
        var item = LegacyPayRunAudit.NewLegacyPayRunAudit("code-964", "name-205");
        Check.Equal(item.Status, LegacyPayRunAuditStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void LegacyPayRunAuditMovesToClosed()
    {
        var item = LegacyPayRunAudit.NewLegacyPayRunAudit("code-964", "name-205");
        try
        {
            item.MoveTo(LegacyPayRunAuditStatus.Closed);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, LegacyPayRunAuditStatus.Closed, "status");
    }

    public static void LegacyPayRunAuditRejectsMovesFromClosed()
    {
        var item = LegacyPayRunAudit.NewLegacyPayRunAudit("code-964", "name-205");
        item.Status = LegacyPayRunAuditStatus.Closed;
        Check.Fails(() => item.MoveTo(LegacyPayRunAuditStatus.Draft), "move");
    }

    public static void RunAll()
    {
        LegacyPayRunAuditStartsInDraft();
        LegacyPayRunAuditMovesToClosed();
        LegacyPayRunAuditRejectsMovesFromClosed();
    }
}
