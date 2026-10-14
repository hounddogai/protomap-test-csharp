namespace Acme.Workforce.Workplace.Tests.FloorPlan.Cycle;

using Acme.Workforce.Workplace.FloorPlan.Cycle;

/// <summary>Checks of cycle code, run by the project's runner.</summary>
public static class FloorPlanCycleChecks
{
    public static void FloorPlanCycleStartsInRequested()
    {
        var item = FloorPlanCycle.NewFloorPlanCycle("code-760", "label-998");
        Check.Equal(item.Status, FloorPlanCycleStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void FloorPlanCycleMovesToRevoked()
    {
        var item = FloorPlanCycle.NewFloorPlanCycle("code-760", "label-998");
        try
        {
            item.MoveTo(FloorPlanCycleStatus.Revoked);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, FloorPlanCycleStatus.Revoked, "status");
    }

    public static void FloorPlanCycleRejectsMovesFromExpired()
    {
        var item = FloorPlanCycle.NewFloorPlanCycle("code-760", "label-998");
        item.Status = FloorPlanCycleStatus.Expired;
        Check.Fails(() => item.MoveTo(FloorPlanCycleStatus.Requested), "move");
    }

    public static void RunAll()
    {
        FloorPlanCycleStartsInRequested();
        FloorPlanCycleMovesToRevoked();
        FloorPlanCycleRejectsMovesFromExpired();
    }
}
