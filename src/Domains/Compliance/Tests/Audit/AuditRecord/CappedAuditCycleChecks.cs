namespace Acme.Workforce.Compliance.Tests.Audit.AuditRecord;

using Acme.Workforce.Compliance.Audit.AuditRecord;

/// <summary>Checks of audit record code, run by the project's runner.</summary>
public static class CappedAuditCycleChecks
{
    public static void CappedAuditCycleStartsInNew()
    {
        var item = CappedAuditCycle.NewCappedAuditCycle("code-770", "label-565");
        Check.Equal(item.Status, CappedAuditCycleStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void CappedAuditCycleMovesToQueued()
    {
        var item = CappedAuditCycle.NewCappedAuditCycle("code-770", "label-565");
        try
        {
            item.MoveTo(CappedAuditCycleStatus.Queued);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, CappedAuditCycleStatus.Queued, "status");
    }

    public static void CappedAuditCycleRejectsMovesFromFailed()
    {
        var item = CappedAuditCycle.NewCappedAuditCycle("code-770", "label-565");
        item.Status = CappedAuditCycleStatus.Failed;
        Check.Fails(() => item.MoveTo(CappedAuditCycleStatus.New), "move");
    }

    public static void RunAll()
    {
        CappedAuditCycleStartsInNew();
        CappedAuditCycleMovesToQueued();
        CappedAuditCycleRejectsMovesFromFailed();
    }
}
