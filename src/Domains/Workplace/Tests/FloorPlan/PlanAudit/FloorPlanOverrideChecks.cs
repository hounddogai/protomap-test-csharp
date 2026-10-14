namespace Acme.Workforce.Workplace.Tests.FloorPlan.PlanAudit;

using Acme.Workforce.Workplace.FloorPlan.PlanAudit;

/// <summary>Checks of plan audit code, run by the project's runner.</summary>
public static class FloorPlanOverrideChecks
{
    public static void FloorPlanOverrideStartsInPlanned()
    {
        var item = FloorPlanOverride.NewFloorPlanOverride("code-935", "label-215");
        Check.Equal(item.Status, FloorPlanOverrideStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void FloorPlanOverrideMovesToScheduled()
    {
        var item = FloorPlanOverride.NewFloorPlanOverride("code-935", "label-215");
        try
        {
            item.MoveTo(FloorPlanOverrideStatus.Scheduled);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, FloorPlanOverrideStatus.Scheduled, "status");
    }

    public static void FloorPlanOverrideRejectsMovesFromCancelled()
    {
        var item = FloorPlanOverride.NewFloorPlanOverride("code-935", "label-215");
        item.Status = FloorPlanOverrideStatus.Cancelled;
        Check.Fails(() => item.MoveTo(FloorPlanOverrideStatus.Planned), "move");
    }

    public static void RunAll()
    {
        FloorPlanOverrideStartsInPlanned();
        FloorPlanOverrideMovesToScheduled();
        FloorPlanOverrideRejectsMovesFromCancelled();
    }
}
