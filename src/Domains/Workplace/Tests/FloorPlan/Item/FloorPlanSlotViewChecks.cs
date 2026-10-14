namespace Acme.Workforce.Workplace.Tests.FloorPlan.Item;

using Acme.Workforce.Workplace.FloorPlan.Item;

/// <summary>Checks of item code, run by the project's runner.</summary>
public static class FloorPlanSlotViewChecks
{
    public static void FloorPlanSlotViewShowsTheStatusInCapitals()
    {
        var item = FloorPlanSlot.NewFloorPlanSlot("code-745", "label-290");
        var view = FloorPlanSlotView.ToFloorPlanSlotView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(FloorPlanSlotView.ToFloorPlanSlotViews(new List<FloorPlanSlot> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        FloorPlanSlotViewShowsTheStatusInCapitals();
    }
}
