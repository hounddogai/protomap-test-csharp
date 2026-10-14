namespace Acme.Workforce.Workplace.Tests.FloorPlan.PlanAdjustment;

using Acme.Workforce.Workplace.FloorPlan.PlanAdjustment;

/// <summary>Checks of plan adjustment code, run by the project's runner.</summary>
public static class AdaptiveFloorPlanAdjustmentViewChecks
{
    public static void AdaptiveFloorPlanAdjustmentViewShowsTheStatusInCapitals()
    {
        var item = AdaptiveFloorPlanAdjustment.NewAdaptiveFloorPlanAdjustment("code-995", "label-81");
        var view = AdaptiveFloorPlanAdjustmentView.ToAdaptiveFloorPlanAdjustmentView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(AdaptiveFloorPlanAdjustmentView.ToAdaptiveFloorPlanAdjustmentViews(new List<AdaptiveFloorPlanAdjustment> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        AdaptiveFloorPlanAdjustmentViewShowsTheStatusInCapitals();
    }
}
