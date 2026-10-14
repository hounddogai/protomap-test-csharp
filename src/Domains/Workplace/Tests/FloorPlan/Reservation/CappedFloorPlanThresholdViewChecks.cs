namespace Acme.Workforce.Workplace.Tests.FloorPlan.Reservation;

using Acme.Workforce.Workplace.FloorPlan.Reservation;

/// <summary>Checks of reservation code, run by the project's runner.</summary>
public static class CappedFloorPlanThresholdViewChecks
{
    public static void CappedFloorPlanThresholdViewShowsTheStatusInCapitals()
    {
        var item = CappedFloorPlanThreshold.NewCappedFloorPlanThreshold("code-898", "label-846");
        var view = CappedFloorPlanThresholdView.ToCappedFloorPlanThresholdView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(CappedFloorPlanThresholdView.ToCappedFloorPlanThresholdViews(new List<CappedFloorPlanThreshold> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        CappedFloorPlanThresholdViewShowsTheStatusInCapitals();
    }
}
