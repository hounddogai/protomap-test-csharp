namespace Acme.Workforce.Workplace.Tests.FloorPlan.Assignment;

using Acme.Workforce.Workplace.FloorPlan.Assignment;

/// <summary>Checks of assignment code, run by the project's runner.</summary>
public static class FloorPlanWindowChecks
{
    public static void FloorPlanWindowStartsInProposed()
    {
        var item = FloorPlanWindow.NewFloorPlanWindow("code-592", "name-638");
        Check.Equal(item.Status, FloorPlanWindowStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void FloorPlanWindowMovesToApproved()
    {
        var item = FloorPlanWindow.NewFloorPlanWindow("code-592", "name-638");
        try
        {
            item.MoveTo(FloorPlanWindowStatus.Approved);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, FloorPlanWindowStatus.Approved, "status");
    }

    public static void FloorPlanWindowRejectsMovesFromWithdrawn()
    {
        var item = FloorPlanWindow.NewFloorPlanWindow("code-592", "name-638");
        item.Status = FloorPlanWindowStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(FloorPlanWindowStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        FloorPlanWindowStartsInProposed();
        FloorPlanWindowMovesToApproved();
        FloorPlanWindowRejectsMovesFromWithdrawn();
    }
}
