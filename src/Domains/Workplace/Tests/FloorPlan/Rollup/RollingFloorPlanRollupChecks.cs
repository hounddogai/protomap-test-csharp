namespace Acme.Workforce.Workplace.Tests.FloorPlan.Rollup;

using Acme.Workforce.Workplace.FloorPlan.Rollup;

/// <summary>Checks of rollup code, run by the project's runner.</summary>
public static class RollingFloorPlanRollupChecks
{
    public static void RollingFloorPlanRollupStartsInProposed()
    {
        var item = RollingFloorPlanRollup.NewRollingFloorPlanRollup("code-119", "name-745");
        Check.Equal(item.Status, RollingFloorPlanRollupStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void RollingFloorPlanRollupMovesToRejected()
    {
        var item = RollingFloorPlanRollup.NewRollingFloorPlanRollup("code-119", "name-745");
        try
        {
            item.MoveTo(RollingFloorPlanRollupStatus.Rejected);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, RollingFloorPlanRollupStatus.Rejected, "status");
    }

    public static void RollingFloorPlanRollupRejectsMovesFromWithdrawn()
    {
        var item = RollingFloorPlanRollup.NewRollingFloorPlanRollup("code-119", "name-745");
        item.Status = RollingFloorPlanRollupStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(RollingFloorPlanRollupStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        RollingFloorPlanRollupStartsInProposed();
        RollingFloorPlanRollupMovesToRejected();
        RollingFloorPlanRollupRejectsMovesFromWithdrawn();
    }
}
