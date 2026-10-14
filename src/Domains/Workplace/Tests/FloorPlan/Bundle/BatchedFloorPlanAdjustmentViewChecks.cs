namespace Acme.Workforce.Workplace.Tests.FloorPlan.Bundle;

using Acme.Workforce.Workplace.FloorPlan.Bundle;

/// <summary>Checks of bundle code, run by the project's runner.</summary>
public static class BatchedFloorPlanAdjustmentViewChecks
{
    public static void BatchedFloorPlanAdjustmentViewShowsTheStatusInCapitals()
    {
        var item = BatchedFloorPlanAdjustment.NewBatchedFloorPlanAdjustment("code-913", "name-230");
        var view = BatchedFloorPlanAdjustmentView.ToBatchedFloorPlanAdjustmentView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(BatchedFloorPlanAdjustmentView.ToBatchedFloorPlanAdjustmentViews(new List<BatchedFloorPlanAdjustment> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        BatchedFloorPlanAdjustmentViewShowsTheStatusInCapitals();
    }
}
