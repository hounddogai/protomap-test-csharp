namespace Acme.Workforce.Workplace.Tests.FloorPlan.Allocation;

using Acme.Workforce.Workplace.FloorPlan.Allocation;

/// <summary>Checks of allocation code, run by the project's runner.</summary>
public static class FloorPlanAllocationChecks
{
    public static void FloorPlanAllocationStartsInPlanned()
    {
        var item = FloorPlanAllocation.NewFloorPlanAllocation("code-584", "label-231");
        Check.Equal(item.Status, FloorPlanAllocationStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void FloorPlanAllocationMovesToScheduled()
    {
        var item = FloorPlanAllocation.NewFloorPlanAllocation("code-584", "label-231");
        try
        {
            item.MoveTo(FloorPlanAllocationStatus.Scheduled);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, FloorPlanAllocationStatus.Scheduled, "status");
    }

    public static void FloorPlanAllocationRejectsMovesFromCancelled()
    {
        var item = FloorPlanAllocation.NewFloorPlanAllocation("code-584", "label-231");
        item.Status = FloorPlanAllocationStatus.Cancelled;
        Check.Fails(() => item.MoveTo(FloorPlanAllocationStatus.Planned), "move");
    }

    public static void RunAll()
    {
        FloorPlanAllocationStartsInPlanned();
        FloorPlanAllocationMovesToScheduled();
        FloorPlanAllocationRejectsMovesFromCancelled();
    }
}
