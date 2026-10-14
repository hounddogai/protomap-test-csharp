namespace Acme.Workforce.Workplace.Tests.FloorPlan.Adjustment;

using Acme.Workforce.Workplace.FloorPlan.Adjustment;

/// <summary>Checks of adjustment code, run by the project's runner.</summary>
public static class FloorPlanReservationViewChecks
{
    public static void FloorPlanReservationViewShowsTheStatusInCapitals()
    {
        var item = FloorPlanReservation.NewFloorPlanReservation("code-977", "label-708");
        var view = FloorPlanReservationView.ToFloorPlanReservationView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(FloorPlanReservationView.ToFloorPlanReservationViews(new List<FloorPlanReservation> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        FloorPlanReservationViewShowsTheStatusInCapitals();
    }
}
