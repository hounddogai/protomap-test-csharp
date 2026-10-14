namespace Acme.Workforce.Workplace.Tests.FloorPlan.Baseline;

using Acme.Workforce.Workplace.FloorPlan.Baseline;

/// <summary>Checks of baseline code, run by the project's runner.</summary>
public static class FloorPlanEstimateChecks
{
    public static void FloorPlanEstimateStartsInDraft()
    {
        var item = FloorPlanEstimate.NewFloorPlanEstimate("code-629", "label-954");
        Check.Equal(item.Status, FloorPlanEstimateStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void FloorPlanEstimateMovesToActive()
    {
        var item = FloorPlanEstimate.NewFloorPlanEstimate("code-629", "label-954");
        try
        {
            item.MoveTo(FloorPlanEstimateStatus.Active);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, FloorPlanEstimateStatus.Active, "status");
    }

    public static void FloorPlanEstimateRejectsMovesFromClosed()
    {
        var item = FloorPlanEstimate.NewFloorPlanEstimate("code-629", "label-954");
        item.Status = FloorPlanEstimateStatus.Closed;
        Check.Fails(() => item.MoveTo(FloorPlanEstimateStatus.Draft), "move");
    }

    public static void RunAll()
    {
        FloorPlanEstimateStartsInDraft();
        FloorPlanEstimateMovesToActive();
        FloorPlanEstimateRejectsMovesFromClosed();
    }
}
