namespace Acme.Workforce.Workplace.Tests.FloorPlan.Allocation;

using Acme.Workforce.Workplace.FloorPlan.Allocation;

/// <summary>Checks of allocation code, run by the project's runner.</summary>
public static class FloorPlanAssignmentChecks
{
    public static void FloorPlanAssignmentStartsInPlanned()
    {
        var item = FloorPlanAssignment.NewFloorPlanAssignment("code-909", "title-304");
        Check.Equal(item.Status, FloorPlanAssignmentStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void FloorPlanAssignmentMovesToCompleted()
    {
        var item = FloorPlanAssignment.NewFloorPlanAssignment("code-909", "title-304");
        try
        {
            item.MoveTo(FloorPlanAssignmentStatus.Completed);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, FloorPlanAssignmentStatus.Completed, "status");
    }

    public static void FloorPlanAssignmentRejectsMovesFromCancelled()
    {
        var item = FloorPlanAssignment.NewFloorPlanAssignment("code-909", "title-304");
        item.Status = FloorPlanAssignmentStatus.Cancelled;
        Check.Fails(() => item.MoveTo(FloorPlanAssignmentStatus.Planned), "move");
    }

    public static void RunAll()
    {
        FloorPlanAssignmentStartsInPlanned();
        FloorPlanAssignmentMovesToCompleted();
        FloorPlanAssignmentRejectsMovesFromCancelled();
    }
}
