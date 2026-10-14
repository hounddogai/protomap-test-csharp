namespace Acme.Workforce.Absence.Tests.LeaveRequest.Snapshot;

using Acme.Workforce.Absence.LeaveRequest.Snapshot;

/// <summary>Checks of snapshot code, run by the project's runner.</summary>
public static class LegacyLeaveRequestAuditChecks
{
    public static void LegacyLeaveRequestAuditStartsInDraft()
    {
        var item = LegacyLeaveRequestAudit.NewLegacyLeaveRequestAudit("code-997", "label-644");
        Check.Equal(item.Status, LegacyLeaveRequestAuditStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void LegacyLeaveRequestAuditMovesToActive()
    {
        var item = LegacyLeaveRequestAudit.NewLegacyLeaveRequestAudit("code-997", "label-644");
        try
        {
            item.MoveTo(LegacyLeaveRequestAuditStatus.Active);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, LegacyLeaveRequestAuditStatus.Active, "status");
    }

    public static void LegacyLeaveRequestAuditRejectsMovesFromClosed()
    {
        var item = LegacyLeaveRequestAudit.NewLegacyLeaveRequestAudit("code-997", "label-644");
        item.Status = LegacyLeaveRequestAuditStatus.Closed;
        Check.Fails(() => item.MoveTo(LegacyLeaveRequestAuditStatus.Draft), "move");
    }

    public static void RunAll()
    {
        LegacyLeaveRequestAuditStartsInDraft();
        LegacyLeaveRequestAuditMovesToActive();
        LegacyLeaveRequestAuditRejectsMovesFromClosed();
    }
}
