namespace Acme.Workforce.Workplace.Tests.FloorPlan.Reservation;

using Acme.Workforce.Workplace.FloorPlan.Reservation;

/// <summary>Checks of reservation code, run by the project's runner.</summary>
public static class PrimaryFloorPlanReservationEngineChecks
{
    public static void PrimaryFloorPlanReservationEngineKeepsResultsWithinBounds()
    {
        var engine = PrimaryFloorPlanReservationEngine.DefaultPrimaryFloorPlanReservationEngine();
        var item = PrimaryFloorPlanReservation.NewPrimaryFloorPlanReservation("code-666", "name-334");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        PrimaryFloorPlanReservationEngineKeepsResultsWithinBounds();
    }
}
