namespace Acme.Workforce.Workplace.Tests.FloorPlan.PlanAudit;

using Acme.Workforce.Workplace.FloorPlan.PlanAudit;

/// <summary>Checks of plan audit code, run by the project's runner.</summary>
public static class LenientFloorPlanAuditChecks
{
    public static void LenientFloorPlanAuditStartsInOpen()
    {
        var item = LenientFloorPlanAudit.NewLenientFloorPlanAudit("code-814", "name-582");
        Check.Equal(item.Status, LenientFloorPlanAuditStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void LenientFloorPlanAuditMovesToReopened()
    {
        var item = LenientFloorPlanAudit.NewLenientFloorPlanAudit("code-814", "name-582");
        try
        {
            item.MoveTo(LenientFloorPlanAuditStatus.Reopened);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, LenientFloorPlanAuditStatus.Reopened, "status");
    }

    public static void LenientFloorPlanAuditRejectsMovesFromArchived()
    {
        var item = LenientFloorPlanAudit.NewLenientFloorPlanAudit("code-814", "name-582");
        item.Status = LenientFloorPlanAuditStatus.Archived;
        Check.Fails(() => item.MoveTo(LenientFloorPlanAuditStatus.Open), "move");
    }

    public static void RunAll()
    {
        LenientFloorPlanAuditStartsInOpen();
        LenientFloorPlanAuditMovesToReopened();
        LenientFloorPlanAuditRejectsMovesFromArchived();
    }
}
