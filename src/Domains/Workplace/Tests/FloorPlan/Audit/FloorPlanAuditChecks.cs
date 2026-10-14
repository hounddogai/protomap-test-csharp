namespace Acme.Workforce.Workplace.Tests.FloorPlan.Audit;

using Acme.Workforce.Workplace.FloorPlan.Audit;

/// <summary>Checks of audit code, run by the project's runner.</summary>
public static class FloorPlanAuditChecks
{
    public static void FloorPlanAuditStartsInPlanned()
    {
        var item = FloorPlanAudit.NewFloorPlanAudit("code-510", "title-824");
        Check.Equal(item.Status, FloorPlanAuditStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void FloorPlanAuditMovesToScheduled()
    {
        var item = FloorPlanAudit.NewFloorPlanAudit("code-510", "title-824");
        try
        {
            item.MoveTo(FloorPlanAuditStatus.Scheduled);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, FloorPlanAuditStatus.Scheduled, "status");
    }

    public static void FloorPlanAuditRejectsMovesFromCancelled()
    {
        var item = FloorPlanAudit.NewFloorPlanAudit("code-510", "title-824");
        item.Status = FloorPlanAuditStatus.Cancelled;
        Check.Fails(() => item.MoveTo(FloorPlanAuditStatus.Planned), "move");
    }

    public static void RunAll()
    {
        FloorPlanAuditStartsInPlanned();
        FloorPlanAuditMovesToScheduled();
        FloorPlanAuditRejectsMovesFromCancelled();
    }
}
