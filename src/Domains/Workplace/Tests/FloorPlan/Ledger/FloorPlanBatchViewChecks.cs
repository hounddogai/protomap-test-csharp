namespace Acme.Workforce.Workplace.Tests.FloorPlan.Ledger;

using Acme.Workforce.Workplace.FloorPlan.Ledger;

/// <summary>Checks of ledger code, run by the project's runner.</summary>
public static class FloorPlanBatchViewChecks
{
    public static void FloorPlanBatchViewShowsTheStatusInCapitals()
    {
        var item = FloorPlanBatch.NewFloorPlanBatch("code-504", "name-614");
        var view = FloorPlanBatchView.ToFloorPlanBatchView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(FloorPlanBatchView.ToFloorPlanBatchViews(new List<FloorPlanBatch> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        FloorPlanBatchViewShowsTheStatusInCapitals();
    }
}
