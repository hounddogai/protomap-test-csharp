namespace Acme.Workforce.Workplace.Tests.FloorPlan.Entry;

using Acme.Workforce.Workplace.FloorPlan.Entry;

/// <summary>Checks of entry code, run by the project's runner.</summary>
public static class ManualFloorPlanReservationViewChecks
{
    public static void ManualFloorPlanReservationViewShowsTheStatusInCapitals()
    {
        var item = ManualFloorPlanReservation.NewManualFloorPlanReservation("code-741", "name-666");
        var view = ManualFloorPlanReservationView.ToManualFloorPlanReservationView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(ManualFloorPlanReservationView.ToManualFloorPlanReservationViews(new List<ManualFloorPlanReservation> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        ManualFloorPlanReservationViewShowsTheStatusInCapitals();
    }
}
