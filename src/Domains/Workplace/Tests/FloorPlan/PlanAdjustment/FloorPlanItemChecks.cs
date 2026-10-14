namespace Acme.Workforce.Workplace.Tests.FloorPlan.PlanAdjustment;

using Acme.Workforce.Workplace.FloorPlan.PlanAdjustment;

/// <summary>Checks of plan adjustment code, run by the project's runner.</summary>
public static class FloorPlanItemChecks
{
    public static void FloorPlanItemStartsInProposed()
    {
        var item = FloorPlanItem.NewFloorPlanItem("code-733", "name-357");
        Check.Equal(item.Status, FloorPlanItemStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void FloorPlanItemMovesToApproved()
    {
        var item = FloorPlanItem.NewFloorPlanItem("code-733", "name-357");
        try
        {
            item.MoveTo(FloorPlanItemStatus.Approved);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, FloorPlanItemStatus.Approved, "status");
    }

    public static void FloorPlanItemRejectsMovesFromWithdrawn()
    {
        var item = FloorPlanItem.NewFloorPlanItem("code-733", "name-357");
        item.Status = FloorPlanItemStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(FloorPlanItemStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        FloorPlanItemStartsInProposed();
        FloorPlanItemMovesToApproved();
        FloorPlanItemRejectsMovesFromWithdrawn();
    }
}
