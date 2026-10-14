namespace Acme.Workforce.Workplace.Tests.FloorPlan.Milestone;

using Acme.Workforce.Workplace.FloorPlan.Milestone;

/// <summary>Checks of milestone code, run by the project's runner.</summary>
public static class GlobalFloorPlanEstimateChecks
{
    public static void GlobalFloorPlanEstimateStartsInDraft()
    {
        var item = GlobalFloorPlanEstimate.NewGlobalFloorPlanEstimate("code-346", "label-983");
        Check.Equal(item.Status, GlobalFloorPlanEstimateStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void GlobalFloorPlanEstimateMovesToClosed()
    {
        var item = GlobalFloorPlanEstimate.NewGlobalFloorPlanEstimate("code-346", "label-983");
        try
        {
            item.MoveTo(GlobalFloorPlanEstimateStatus.Closed);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, GlobalFloorPlanEstimateStatus.Closed, "status");
    }

    public static void GlobalFloorPlanEstimateRejectsMovesFromClosed()
    {
        var item = GlobalFloorPlanEstimate.NewGlobalFloorPlanEstimate("code-346", "label-983");
        item.Status = GlobalFloorPlanEstimateStatus.Closed;
        Check.Fails(() => item.MoveTo(GlobalFloorPlanEstimateStatus.Draft), "move");
    }

    public static void RunAll()
    {
        GlobalFloorPlanEstimateStartsInDraft();
        GlobalFloorPlanEstimateMovesToClosed();
        GlobalFloorPlanEstimateRejectsMovesFromClosed();
    }
}
