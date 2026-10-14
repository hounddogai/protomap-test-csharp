namespace Acme.Workforce.Workplace.Tests.FloorPlan.Item;

using Acme.Workforce.Workplace.FloorPlan.Item;

/// <summary>Checks of item code, run by the project's runner.</summary>
public static class ManualFloorPlanItemViewChecks
{
    public static void ManualFloorPlanItemViewShowsTheStatusInCapitals()
    {
        var item = ManualFloorPlanItem.NewManualFloorPlanItem("code-85", "title-904");
        var view = ManualFloorPlanItemView.ToManualFloorPlanItemView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(ManualFloorPlanItemView.ToManualFloorPlanItemViews(new List<ManualFloorPlanItem> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        ManualFloorPlanItemViewShowsTheStatusInCapitals();
    }
}
