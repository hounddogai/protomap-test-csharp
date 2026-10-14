namespace Acme.Workforce.Workplace.Tests.FloorPlan.Bundle;

using Acme.Workforce.Workplace.FloorPlan.Bundle;

/// <summary>Checks of bundle code, run by the project's runner.</summary>
public static class BatchedFloorPlanAdjustmentChecks
{
    public static void BatchedFloorPlanAdjustmentStartsInIdle()
    {
        var item = BatchedFloorPlanAdjustment.NewBatchedFloorPlanAdjustment("code-913", "name-230");
        Check.Equal(item.Status, BatchedFloorPlanAdjustmentStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void BatchedFloorPlanAdjustmentMovesToReady()
    {
        var item = BatchedFloorPlanAdjustment.NewBatchedFloorPlanAdjustment("code-913", "name-230");
        try
        {
            item.MoveTo(BatchedFloorPlanAdjustmentStatus.Ready);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, BatchedFloorPlanAdjustmentStatus.Ready, "status");
    }

    public static void BatchedFloorPlanAdjustmentRejectsMovesFromStopped()
    {
        var item = BatchedFloorPlanAdjustment.NewBatchedFloorPlanAdjustment("code-913", "name-230");
        item.Status = BatchedFloorPlanAdjustmentStatus.Stopped;
        Check.Fails(() => item.MoveTo(BatchedFloorPlanAdjustmentStatus.Idle), "move");
    }

    public static void RunAll()
    {
        BatchedFloorPlanAdjustmentStartsInIdle();
        BatchedFloorPlanAdjustmentMovesToReady();
        BatchedFloorPlanAdjustmentRejectsMovesFromStopped();
    }
}
