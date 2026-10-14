namespace Acme.Workforce.Workplace.Tests.FloorPlan.Threshold;

using Acme.Workforce.Workplace.FloorPlan.Threshold;

/// <summary>Checks of threshold code, run by the project's runner.</summary>
public static class FloorPlanThresholdChecks
{
    public static void FloorPlanThresholdStartsInNew()
    {
        var item = FloorPlanThreshold.NewFloorPlanThreshold("code-250", "title-77");
        Check.Equal(item.Status, FloorPlanThresholdStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void FloorPlanThresholdMovesToQueued()
    {
        var item = FloorPlanThreshold.NewFloorPlanThreshold("code-250", "title-77");
        try
        {
            item.MoveTo(FloorPlanThresholdStatus.Queued);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, FloorPlanThresholdStatus.Queued, "status");
    }

    public static void FloorPlanThresholdRejectsMovesFromFailed()
    {
        var item = FloorPlanThreshold.NewFloorPlanThreshold("code-250", "title-77");
        item.Status = FloorPlanThresholdStatus.Failed;
        Check.Fails(() => item.MoveTo(FloorPlanThresholdStatus.New), "move");
    }

    public static void RunAll()
    {
        FloorPlanThresholdStartsInNew();
        FloorPlanThresholdMovesToQueued();
        FloorPlanThresholdRejectsMovesFromFailed();
    }
}
