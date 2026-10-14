namespace Acme.Workforce.Compliance.Tests.Audit.RecordItem;

using Acme.Workforce.Compliance.Audit.RecordItem;

/// <summary>Checks of record item code, run by the project's runner.</summary>
public static class CappedAuditPlanChecks
{
    public static void CappedAuditPlanStartsInIdle()
    {
        var item = CappedAuditPlan.NewCappedAuditPlan("code-43", "label-39");
        Check.Equal(item.Status, CappedAuditPlanStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void CappedAuditPlanMovesToWarming()
    {
        var item = CappedAuditPlan.NewCappedAuditPlan("code-43", "label-39");
        try
        {
            item.MoveTo(CappedAuditPlanStatus.Warming);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, CappedAuditPlanStatus.Warming, "status");
    }

    public static void CappedAuditPlanRejectsMovesFromStopped()
    {
        var item = CappedAuditPlan.NewCappedAuditPlan("code-43", "label-39");
        item.Status = CappedAuditPlanStatus.Stopped;
        Check.Fails(() => item.MoveTo(CappedAuditPlanStatus.Idle), "move");
    }

    public static void RunAll()
    {
        CappedAuditPlanStartsInIdle();
        CappedAuditPlanMovesToWarming();
        CappedAuditPlanRejectsMovesFromStopped();
    }
}
