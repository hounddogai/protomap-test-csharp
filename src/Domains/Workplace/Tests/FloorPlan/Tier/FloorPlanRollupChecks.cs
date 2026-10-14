namespace Acme.Workforce.Workplace.Tests.FloorPlan.Tier;

using Acme.Workforce.Workplace.FloorPlan.Tier;

/// <summary>Checks of tier code, run by the project's runner.</summary>
public static class FloorPlanRollupChecks
{
    public static void FloorPlanRollupStartsInNew()
    {
        var item = FloorPlanRollup.NewFloorPlanRollup("code-685", "name-476");
        Check.Equal(item.Status, FloorPlanRollupStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void FloorPlanRollupMovesToRunning()
    {
        var item = FloorPlanRollup.NewFloorPlanRollup("code-685", "name-476");
        try
        {
            item.MoveTo(FloorPlanRollupStatus.Running);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, FloorPlanRollupStatus.Running, "status");
    }

    public static void FloorPlanRollupRejectsMovesFromFailed()
    {
        var item = FloorPlanRollup.NewFloorPlanRollup("code-685", "name-476");
        item.Status = FloorPlanRollupStatus.Failed;
        Check.Fails(() => item.MoveTo(FloorPlanRollupStatus.New), "move");
    }

    public static void RunAll()
    {
        FloorPlanRollupStartsInNew();
        FloorPlanRollupMovesToRunning();
        FloorPlanRollupRejectsMovesFromFailed();
    }
}
